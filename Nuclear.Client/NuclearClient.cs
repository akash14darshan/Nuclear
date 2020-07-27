using Nuclear.Common;
using Nuclear.Common.Proxy;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;

namespace Nuclear.Client
{
	public abstract class NuclearClient : UDP
	{
		public NuclearClient(string host, int port)
		{
			HostAddress = host;
			Port = port;
			Endpoint = new IPEndPoint(IPAddress.Any, port);
		}
		public int Ping { get; set; }
		public int PendingRequests => Packets.Count;

		protected void Connect()
		{
			BeginConnect();
			BeginReceive();
			BeginPing();
			BeginProcess();
			lastUpdateTime = DateTime.Now;
		}

		public override void Dispose()
		{
			base.Dispose();
			Status = ConnectionStatus.DisconnectOnRequest;
		}
		protected abstract void OnException(Exception ex);
		protected abstract void OnConnected();
		protected abstract void OnDisconnected();
		protected virtual void OnDisableAndDisconnect(){/*Space*/}
		public ConnectionStatus Status
		{
			get => _status;
			set
			{
				if(_status == value)
				{
					return;
				}
				switch(value)
				{
					case ConnectionStatus.Connected:
						if(_status!=ConnectionStatus.Connected)
							OnConnected();
						break;
					case ConnectionStatus.DisconnectOnRequest:
						break;
					case ConnectionStatus.Disconnected:
						OnDisconnected();
						break;
				}
				_status = value;
			}
		}

		protected abstract void OnEvent(byte OpCode, byte[] data);

		protected bool HandleAllResponses()
		{
			bool Response = false;
			while(true)
			{
				if (TryDequeue(out byte[] data))
				{
					Response = true;
					try
					{
						byte OpCode = data[0];
						byte[] response = new byte[data.Length - 1];
						Array.Copy(data, 1, response, 0, response.Length);
						OnEvent(OpCode, response);
					}
					catch(Exception e)
					{
						OnException(e);
					}
				}
				else break;
			}
			return Response;
		}

		protected bool HandleResponse()
		{
			if(TryDequeue(out byte[] data))
			{
				byte OpCode = data[0];
				byte[] response = new byte[data.Length - 1];
				Array.Copy(data, 1, response, 0, response.Length);
				OnEvent(OpCode, response);
				return true;
			}
			return false;
		}

		bool IsPingPacket(Packet packet)
		{
			return packet!=null && packet.Data != null && packet.Data[0] == 0xFF;
		}

		void BeginPing()
		{
			new Thread(() =>
			{
				while (Status != ConnectionStatus.DisconnectOnRequest)
				{
					try
					{
						SendPing();
						Thread.Sleep(1000);
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
					}
				}
			}).Start();
		}

		void UpdatePing(Packet packet)
		{
			try
			{
				using (var memoryStream = new MemoryStream(packet.Data))
				{
					ByteProxy.Deserialize(memoryStream);
					int retvalue = Int32Proxy.Deserialize(memoryStream);
					if (retvalue != 0)
					{
						Ping = Environment.TickCount - retvalue;
						lastUpdateTime = DateTime.Now;
						Status = ConnectionStatus.Connected;
					}
				}
			}
			catch(Exception e) { Console.WriteLine(e); }
		}

		bool IsConnected()
		{
			return (DateTime.Now - lastUpdateTime).TotalSeconds < DisconnectionTimeInSecs && Status!=ConnectionStatus.DisconnectOnRequest;
		}

		void BeginConnect()
		{
			_client.Connect(HostAddress,Port);
			lastUpdateTime = DateTime.Now;
			SendPing();
		}

		private bool SendDirect(byte[] data)
		{
			if (Status == ConnectionStatus.DisconnectOnRequest)
				return false;
			if (!IsConnected())
			{
				RenewClient();
				BeginConnect();
			}
			if(Status == ConnectionStatus.Connected)
			{
				_client.Send(data, data.Length);
				return true;
			}
			return false;
		}

		protected bool Send(byte Opcode,MemoryStream bytes)
		{
			using(MemoryStream stream = new MemoryStream())
			{
				ByteProxy.Serialize(stream, 0xEE);
				ByteProxy.Serialize(stream, Opcode);
				bytes.WriteTo(stream);
				byte[] toSend = stream.ToArray();
				stream.Dispose();
				return SendDirect(toSend);
			}
		}

		void SendPing()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				ByteProxy.Serialize(stream, 0xFF);
				Int32Proxy.Serialize(stream, Environment.TickCount);
				byte[] ping = stream.ToArray();
				_client.Send(ping, ping.Length);
			}
		}

		void DisableAndDisconnect(byte[] data)
		{
			MemoryStream stream = new MemoryStream(data);
			ByteProxy.Deserialize(stream);
			Status = ConnectionStatus.DisconnectOnRequest;
			_client.Close();
			Dispose();
			OnDisableAndDisconnect();
		}

		bool TryDequeue(out byte[] data)
		{
			return Packets.TryDequeue(out data);
		}


		bool IsDisconnectPacket(Packet packet)
		{
			return packet != null && packet.Data != null && packet.Data[0] == 0xFE;
		}

		bool IsValidPacket(Packet packet)
		{
			return packet != null && packet.Data != null && packet.Data[0] == 0xFD;
		}

		void BeginProcess()
		{
			new Thread(() =>
			{
				while (Status != ConnectionStatus.DisconnectOnRequest)
				{
					if (!ToBeProcessed.TryDequeue(out Packet packet))
					{
						Process.Reset(); Process.WaitOne();
						continue;
					}
					if (IsPingPacket(packet))
					{
						UpdatePing(packet);
						if (Status != ConnectionStatus.Connected)
						{
							Status = ConnectionStatus.Connected;
							OnConnected();
						}
					}
					else if (IsDisconnectPacket(packet))
					{
						DisableAndDisconnect(packet.Data);
					}
					else if (IsValidPacket(packet))
					{
						byte[] ToEnqueue = new byte[packet.Data.Length - 1];
						Array.Copy(packet.Data, 1, ToEnqueue, 0, ToEnqueue.Length);
						Packets.Enqueue(ToEnqueue);
					}
					else if (PartsManager.IsPartData(packet) && PartsManager.HandlePart(packet, out byte[] data))
					{
						Packets.Enqueue(data);
					}
				}
			}).Start();
		}

		void BeginReceive()
		{
			new Thread(async() =>
			{
				while (Status != ConnectionStatus.DisconnectOnRequest)
				{
					try
					{
						Packet packet = Receive(ref Endpoint);
						ToBeProcessed.Enqueue(packet);
						Process.Set();
					}
					catch (Exception ex)
					{
						if (ex is ObjectDisposedException || ex is SocketException || ex is InvalidOperationException)
						{
							if (Status == ConnectionStatus.Connected)
							{
								Status = ConnectionStatus.Disconnected;
							}
						}
						else Console.WriteLine("Client exception:\n" + ex);
					}
				}
			}).Start();
		}

		readonly PartsManager PartsManager = new PartsManager();
		readonly string HostAddress;
		readonly int Port;
		ConnectionStatus _status = ConnectionStatus.Notconnected;
		readonly int DisconnectionTimeInSecs = 5;
		IPEndPoint Endpoint;
		DateTime lastUpdateTime;
		ManualResetEvent Process = new ManualResetEvent(false);
		readonly ConcurrentQueue<Packet> ToBeProcessed = new ConcurrentQueue<Packet>();
		readonly ConcurrentQueue<byte[]> Packets = new ConcurrentQueue<byte[]>();
	}
}
