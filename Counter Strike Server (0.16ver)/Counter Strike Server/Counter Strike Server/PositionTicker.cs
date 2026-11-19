// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Counter_Strike_Server
{
    /// <summary>
    /// Sends the positions of the players by "ticks".
    ///
    /// Before, every received position was sent at once to every other player
    /// of the party: with 10 players, each DS received up to 9 x 20 = 180
    /// small packets per second. Now the server keeps the newest position of
    /// each player and, 20 times per second, sends to each client ONE packet
    /// with all the positions that changed. Each DS receives 20 packets per
    /// second, whatever the number of players. The positions wait 25 ms on
    /// average, the prediction of the game hides it.
    /// </summary>
    public static class PositionTicker
    {
        /// <summary>
        /// Time between two ticks (ms). 50 = 20 per second, like the game.
        /// </summary>
        public const int TICK_MS = 50;

        public static void Start()
        {
            Thread thread = new(Run);
            thread.IsBackground = true;
            thread.Start();
        }

        private static void Run()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            long nextTick = TICK_MS;

            while (true)
            {
                // Sleep until the next tick (Thread.Sleep alone drifts)
                long wait = nextTick - stopwatch.ElapsedMilliseconds;
                if (wait > 0)
                    Thread.Sleep((int)wait);
                nextTick += TICK_MS;
                if (nextTick < stopwatch.ElapsedMilliseconds)
                    nextTick = stopwatch.ElapsedMilliseconds + TICK_MS; // Late, don't try to catch up

                try
                {
                    Tick();
                }
                catch (Exception e)
                {
                    Logger.LogErrorInFile($"Position tick error: {e.Message} {e.StackTrace}");
                }
            }
        }

        private static void Tick()
        {
            List<Party> parties;
            lock (PartyManager.allParties)
            {
                parties = new List<Party>(PartyManager.allParties);
            }

            lock (ConnectionManager.allClients)
            {
                foreach (Party party in parties)
                {
                    List<Client> clients = new(party.allConnectedClients);

                    // Players whose position changed since the last tick
                    List<Client> moved = new();
                    foreach (Client client in clients)
                    {
                        if (client.positionChanged)
                        {
                            client.positionChanged = false;
                            moved.Add(client);
                        }
                    }

                    //Players on a gun on the floor take it (all the players: the
                    //player who dropped a gun can take it back later without moving)
                    party.CheckDroppedGunsPickup(clients);

                    if (moved.Count == 0)
                        continue;

                    foreach (Client destination in clients)
                    {
                        if (destination.removed)
                            continue;

                        bool useUdp = UdpManager.CanUse(destination);
                        StringBuilder udpPackets = new();

                        foreach (Client client in moved)
                        {
                            if (client == destination)
                                continue; // A player doesn't get his own position

                            string request = client.communicator.GetPositionRequest();
                            if (useUdp)
                                udpPackets.Append('{').Append(request).Append('}');
                            else
                                Call.Create(request, destination); // Grouped by NetworkSender
                        }

                        if (useUdp && udpPackets.Length != 0)
                        {
                            UserInterfaceManager.PrintOutData(destination, "UDP " + udpPackets);
                            UdpManager.Send(destination, udpPackets.ToString());
                        }
                    }
                }
            }
        }
    }
}
