using Nuclear.Common;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;

namespace Nuclear.Server
{
    internal class PacketManager : IDisposable
    {
        internal PacketManager(ConcurrentQueue<Operation> Operations,Func<IPEndPoint,Peer> Getpeer,Action<Exception> Exception)
        {
			OperationQueue = Operations;
			GetPeer = Getpeer;
			OnException += Exception;
			PacketHandler();
        }

		public void Dispose()
		{
			OnException = null;
			GetPeer = null;
			Disposed = true;
		}

		void PacketHandler()
		{
			new Thread(() =>
			{
				while (!Disposed)
				{
					try
					{
						if (PacketQueue.TryDequeue(out Packet packet))
						{
							OperationQueue.Enqueue(new Operation(GetPeer(packet.EndPoint), packet));
						}
						else { PacketQueueBegin.Reset(); PacketQueueBegin.WaitOne(); }
					}
					catch(Exception e)
					{
						OnException(e);
					}
				}
				Console.WriteLine("Packet manager disposed");
			}).Start();
		}

		public bool IsValidPacket(Packet packet)
		{
			return packet!= null && packet.Data!=null && packet.Data[0] == 0xEE;
		}

		public void EnqueuePacket(Packet packet)
		{
			PacketQueueBegin.Set();
			PacketQueue.Enqueue(packet);
		}

		bool Disposed = false;
		Func<IPEndPoint, Peer> GetPeer;
		Action<Exception> OnException = new Action<Exception>((Exception e) => { });
		readonly ConcurrentQueue<Operation> OperationQueue;
		readonly ConcurrentQueue<Packet> PacketQueue = new ConcurrentQueue<Packet>();
		readonly ManualResetEvent PacketQueueBegin = new ManualResetEvent(false);
	}
}
