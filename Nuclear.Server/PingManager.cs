using Nuclear.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Nuclear.Server
{
    public class PingManager : IDisposable
    {
        public PingManager(Func<IPEndPoint,Peer> Getpeer,Func<Packet,Peer> Createpeer,Action<Exception> Exception)
        {
            GetPeer = Getpeer;
            CreatePeer = Createpeer;
            OnException += Exception;
            PingHandler();
        }

        public void Dispose()
        {
            OnException = null;
            Disposed = true;
        }

        void PingHandler()
        {
            new Thread(() =>
            {
                while (!Disposed)
                {
                    try
                    {
                        if (DequeuePing(out Packet pingpacket))
                        {
                            Peer peer = GetPeer(pingpacket.EndPoint);
                            if (peer == null) { peer = CreatePeer(pingpacket); }
                            peer.EndPoint = pingpacket.EndPoint;
                            peer.RenewLastUpdate();
                            peer.SendRaw(pingpacket.Data);
                        }
                        else { Begin.Reset(); Begin.WaitOne(); }
                    }
                    catch (Exception e)
                    {
                        OnException(e);
                    }
                }
                Console.WriteLine("Ping manager disposed");
            }).Start();
        }

        public bool IsPingPacket(Packet packet)
        {
            return packet.Data != null && packet.Data!=null && packet.Data[0] == 0xFF;
        }

        public void EnqueuePing(Packet packet)
        {
            Begin.Set();
            PingQueue.Enqueue(packet);
        }

        bool DequeuePing(out Packet packet)
        {
            packet = null;
            if (PingQueue.IsEmpty)
                return false;
            return PingQueue.TryDequeue(out packet);
        }

        bool Disposed = false;
        Func<IPEndPoint, Peer> GetPeer;
        Func<Packet, Peer> CreatePeer;
        Action<Exception> OnException = new Action<Exception>((Exception e) => { });
        ManualResetEvent Begin = new ManualResetEvent(false);
        readonly ConcurrentQueue<Packet> PingQueue = new ConcurrentQueue<Packet>();
    }
}
