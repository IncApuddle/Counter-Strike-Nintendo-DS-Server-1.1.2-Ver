// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Counter_Strike_Server
{
    /// <summary>
    /// Player positions over UDP.
    ///
    /// With TCP, if one packet is lost on the wifi of a DS, all the following
    /// packets wait until the lost one is sent again. For positions this is
    /// useless: only the newest position matters. Positions are sent over UDP
    /// (a lost one is replaced by the next one), everything else stays on TCP.
    ///
    /// How it works:
    /// 1. After the KEY, the server sends "UDP;port;token" over TCP.
    /// 2. The game sends datagrams starting with "{T;token;sequence}". The
    ///    token tells which client it is, the sequence is used to drop old
    ///    datagrams that arrive late.
    /// 3. On the first datagram, the server learns the address of the client
    ///    (it can be changed by a router, NAT) and sends "UDPOK" over TCP.
    ///    From then, the game sends its position over UDP.
    /// 4. The server sends the positions of the other players to this client
    ///    over UDP ("{S;sequence}{POS;...}"), while it receives datagrams from
    ///    it (the game sends one every second even when it doesn't move).
    ///    Without UDP (blocked by a firewall...), TCP is used like before.
    /// </summary>
    public static class UdpManager
    {
        private static UdpClient udp;
        private static int udpPort;
        private static readonly ConcurrentDictionary<int, Client> clientsByToken = new();
        private static readonly Random rand = new();

        /// <summary>
        /// Seconds without datagram from a client before using TCP again
        /// </summary>
        private const double UDP_TIMEOUT_SECONDS = 5;

        /// <summary>
        /// Start the UDP socket (same port number as TCP)
        /// </summary>
        public static void Start(int port)
        {
            udpPort = port;
            udp = new UdpClient(new IPEndPoint(IPAddress.Any, port));

            // On Windows, a datagram sent to a closed port makes the next
            // Receive() fail with "connection reset". Disable that.
            if (OperatingSystem.IsWindows())
            {
                const int SIO_UDP_CONNRESET = -1744830452;
                udp.Client.IOControl(SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
            }

            Thread receiveThread = new(ReceiveLoop);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        /// <summary>
        /// Give a token to a new client and send it the UDP information
        /// </summary>
        public static void Register(Client client)
        {
            if (udp == null)
                return;

            int token;
            do
            {
                lock (rand)
                {
                    token = rand.Next(1, int.MaxValue);
                }
            } while (!clientsByToken.TryAdd(token, client));

            client.udpToken = token;
            Call.Create($"UDP;{udpPort};{token}", client);
        }

        /// <summary>
        /// Forget a removed client
        /// </summary>
        public static void Unregister(Client client)
        {
            if (client.udpToken != 0)
                clientsByToken.TryRemove(client.udpToken, out _);
            client.udpConfirmed = false;
            client.udpEndPoint = null;
        }

        /// <summary>
        /// Can positions be sent to this client over UDP?
        /// </summary>
        public static bool CanUse(Client client)
        {
            return udp != null && client.udpConfirmed && client.udpEndPoint != null &&
                   (DateTime.Now - client.udpLastReceive).TotalSeconds < UDP_TIMEOUT_SECONDS;
        }

        /// <summary>
        /// Send packets (already with "{}") to a client over UDP
        /// </summary>
        public static void Send(Client client, string packets)
        {
            IPEndPoint endPoint = client.udpEndPoint;
            if (endPoint == null)
                return;

            int sequence = Interlocked.Increment(ref client.udpSendSequence);
            byte[] bytes = Encoding.ASCII.GetBytes("{S;" + sequence + "}" + packets);
            try
            {
                udp.Send(bytes, bytes.Length, endPoint);
            }
            catch (Exception)
            {
                // A lost datagram is not a problem
            }
        }

        private static void ReceiveLoop()
        {
            while (true)
            {
                IPEndPoint remote = new(IPAddress.Any, 0);
                byte[] data;
                try
                {
                    data = udp.Receive(ref remote);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception)
                {
                    continue;
                }

                try
                {
                    HandleDatagram(Encoding.ASCII.GetString(data), remote);
                }
                catch (Exception e)
                {
                    Logger.LogErrorInFile($"UDP error: {e.Message} {e.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Split "{a;b}{c;d}" into packets
        /// </summary>
        private static List<string[]> SplitPackets(string text)
        {
            List<string[]> packets = new();
            int index = 0;
            while (true)
            {
                int start = text.IndexOf('{', index);
                if (start == -1)
                    break;
                int end = text.IndexOf('}', start + 1);
                if (end == -1)
                    break;
                packets.Add(text.Substring(start + 1, end - start - 1).Split(';'));
                index = end + 1;
            }
            return packets;
        }

        private static void HandleDatagram(string text, IPEndPoint remote)
        {
            List<string[]> packets = SplitPackets(text);
            if (packets.Count == 0)
                return;

            // Header: {T;token;sequence}
            string[] header = packets[0];
            if (header.Length < 3 || header[0] != "T")
                return;
            if (!int.TryParse(header[1], out int token) || !int.TryParse(header[2], out int sequence))
                return;
            if (!clientsByToken.TryGetValue(token, out Client client))
                return;

            // Same lock as the TCP read thread (the client data is changed)
            lock (ConnectionManager.allClients)
            {
                if (client.removed)
                    return;

                // Old datagram (arrived after a newer one): ignore it.
                // A much smaller number = the game restarted its counter.
                if (client.udpLastReceivedSequence != 0 && sequence <= client.udpLastReceivedSequence &&
                    client.udpLastReceivedSequence - sequence < 100000)
                    return;

                client.udpLastReceivedSequence = sequence;
                client.udpEndPoint = new IPEndPoint(remote.Address, remote.Port);
                client.udpLastReceive = DateTime.Now;

                if (!client.udpConfirmed)
                {
                    client.udpConfirmed = true;
                    Call.Create("UDPOK", client);
                    UserInterfaceManager.PrintMessage($"Client {client.id} uses UDP ({remote})");
                }

                // Only positions are accepted over UDP
                for (int i = 1; i < packets.Count; i++)
                {
                    string[] packet = packets[i];
                    if (packet.Length >= 6 && packet[0] == ((int)Router.RequestType.POS).ToString() &&
                        client.checkedKey && client.party != null)
                    {
                        UserInterfaceManager.PrintInData(client, "UDP " + string.Join(';', packet));
                        client.SetPosition(packet[1], packet[2], packet[3], packet[4], packet[5]);
                    }
                }
            }
        }
    }
}
