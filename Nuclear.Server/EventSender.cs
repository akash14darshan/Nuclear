using Nuclear.Common;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace Nuclear.Server
{
    internal class EventSender
    {
        internal EventSender(UdpClient client)
        {
            Client = client;
            SendThread();
        }

        void SendThread()
        {
            new Thread(() =>
            {
                while(true)
                {
                    if(SendQueue.TryDequeue(out Packet data))
                    {
                        Client.Send(data.Data, data.Data.Length, data.EndPoint);
                    }
                    else { BeginSend.Reset(); BeginSend.WaitOne(); }
                }
            }).Start();
        }

        internal void Enqueue(Packet data)
        {
            BeginSend.Set();
            SendQueue.Enqueue(data);
        }

        readonly UdpClient Client;
        readonly ManualResetEvent BeginSend = new ManualResetEvent(false);
        readonly ConcurrentQueue<Packet> SendQueue = new ConcurrentQueue<Packet>();
    }
}
