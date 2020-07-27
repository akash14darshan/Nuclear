using Nuclear.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Nuclear.Server
{
	public abstract class NuclearServer : UDP
	{
		protected List<Peer> Peers = new List<Peer>();
		protected object PeerLock = new object();
		protected TickManager TickManager;
		private bool Disposed = false;
		private IPEndPoint EndPoint;
		private EventSender EventSender;
		protected NuclearServer(int port,int tickrate) : this(new IPEndPoint(IPAddress.Any, port))
		{
			PacketManager = new PacketManager(Workloads,GetPeer,OnException);
			PingManager = new PingManager(GetPeer,CreatePeer,OnException);
			TickManager = new TickManager(tickrate, CleanPeers);
			TickManager.AddToTick(HandlePackets);
			TickManager.AddToTick(OnTick);
			EventSender = new EventSender(_client);
			BeginListen();
			SetMaxThreads();
		}
		public NuclearServer(IPEndPoint endpoint)
		{
			_client = new UdpClient(endpoint);
			EndPoint = endpoint;
		}
		protected abstract void OnPeerJoin(Peer peer);
		protected abstract void OnPeerLeave(Peer peer);
		protected abstract void OnTick();
		protected abstract void OnException(Exception exception);
		protected abstract void OnEvent(Peer peer,byte OpCode, MemoryStream data);
		public override void Dispose()
		{
			Disposed = true;
			base.Dispose();
			Peers.Clear();
			TickManager.Dispose();
			PingManager.Dispose();
			PacketManager.Dispose();
		}

		void SetMaxThreads()
		{
			ThreadPool.GetMinThreads(out _, out int minIOC);
			if (!ThreadPool.SetMaxThreads(15000, minIOC))
			{
				Console.WriteLine("Setting max thread failed");
			}
			if (!ThreadPool.SetMinThreads(15000, minIOC))
			{
				Console.WriteLine("Setting min thread failed");
			}
		}

		void BeginListen()
		{
			new Thread(() => {
				while (!Disposed)
				{
					try
					{
						Packet packet = Receive(ref EndPoint);
						if (PingManager.IsPingPacket(packet))
						{
							PingManager.EnqueuePing(packet);
						}
						else if(PacketManager.IsValidPacket(packet))
						{
							PacketManager.EnqueuePacket(packet);
						}
					}
					catch(ObjectDisposedException){}
					catch (SocketException) {}
					catch(Exception e)
					{
						Console.WriteLine(e);
					}
				}
				Console.WriteLine("Listener has stopped");
			}).Start();
		}

		protected void TestSendAll(byte[] data,int count)
		{
			if(Peers.Count == 0)
			{
				Console.WriteLine("No peer has initiated connection, sending message aborted");
				return;
			}
			foreach (var peer in Peers)
            {
				peer.SendEvent(data);
			}
			Console.WriteLine("Sent "+count);
		}

		private Peer CreatePeer(Packet packet)
		{
			Peer peer = new Peer(packet.EndPoint);
			peer.SetEventSender(EventSender);
			lock (PeerLock)
			{
				Peers.Add(peer);
				OnPeerJoin(peer);
			}
			return peer;
		}

		private Peer GetPeer(IPEndPoint endpoint)
		{
			lock(PeerLock)
			{
				foreach(var peer in Peers)
				{
					if (peer.EndPoint.ToString() == endpoint.ToString())
						return peer;
				}
				return null;
			}
		}

		private void HandlePackets()
		{
			do
			{
				if (Workloads.TryDequeue(out Operation result) && result!=null)
				{
					OnEvent(result.Peer,result.OpCode, result.Data);
				}
			} while (!Workloads.IsEmpty);
		}

		private void CleanPeers()
		{
			lock(PeerLock)
			{
				for(int i=0;i<Peers.Count;i++)
				{
					if(!Peers[i].IsAlive() || Peers[i].HasError)
					{
						Peer peer = Peers[i];
						Peers.RemoveAt(i);
						OnPeerLeave(peer);
						peer.Dispose();
					}
				}
			}
		}

		readonly ConcurrentQueue<Operation> Workloads = new ConcurrentQueue<Operation>();
		PingManager PingManager;
		PacketManager PacketManager;
	}
}
