// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)

using System.Collections.Concurrent;
using System.Threading;

namespace Counter_Strike_Server
{
    /// <summary>
    /// Sends the queued data of the clients from one thread.
    ///
    /// Before, every thread (timer, ping, physics, read) wrote to the client
    /// sockets itself: two threads could write to the same socket at the same
    /// time, and every message was a separate TCP packet. Now the messages are
    /// added to the client's buffer (Client.QueueSend) and this thread sends
    /// everything that is waiting in one write per client. It wakes up as soon
    /// as there is something to send (no fixed delay).
    /// </summary>
    public static class NetworkSender
    {
        private static readonly AutoResetEvent dataQueued = new(false);
        private static readonly ConcurrentQueue<Client> clientsToFlush = new();

        /// <summary>
        /// Called when data is added to a client's buffer
        /// </summary>
        public static void Notify(Client client)
        {
            // Add the client to the queue only once
            if (Interlocked.Exchange(ref client.sendQueued, 1) == 0)
                clientsToFlush.Enqueue(client);

            dataQueued.Set();
        }

        /// <summary>
        /// Thread that sends the data
        /// </summary>
        public static void Run()
        {
            while (true)
            {
                dataQueued.WaitOne(100);

                while (clientsToFlush.TryDequeue(out Client client))
                {
                    Interlocked.Exchange(ref client.sendQueued, 0);
                    client.FlushSendBuffer();
                }
            }
        }
    }
}
