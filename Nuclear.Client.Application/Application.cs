using Nuclear.Common.Proxy;
using System;
using System.IO;
using System.Threading;

namespace Nuclear.Client.Application
{
    public class Application : NuclearClient
    {
		public Application(string host,int port) : base(host,port)
		{
			//add implementation in client to constantly call HandleResponse, or HandleAllResponses
			BeginConnect();
			TestReceiveLoop();
			TestSending();
		}

		void TestSending()
		{
			Console.WriteLine("Whatever value you type in now, will be sent to server\n");
			while (true)
			{
				var input = Console.ReadLine();
				using (var stream = new MemoryStream())
				{
					StringProxy.Serialize(stream, input);
					Send(1, stream);
				}
			}
		}

		void TestReceiveLoop() 
		{
			new Thread(() =>
			{
				while(true)
				{
					HandleAllResponses();
					Thread.Sleep(1);
				}
			}).Start();
		}

		public void BeginConnect()
		{
			//automatically connects automatically when disconnected. Doesnt connect back when server force closes connection.
			//Stops connecting automatically when disposed.
			Connect();
			Console.WriteLine("Further implementations here");
		}

		protected override void OnConnected()
		{
			//not thread safe
			//occurs when client is connected to server, or reconnected back.
			Console.WriteLine("Connected to server");
		}

		protected override void OnException(Exception ex)
		{
			//not thread safe
			//occurs at internal exception, or at OnEvent
			Console.WriteLine("Exception");
		}

		protected override void OnDisconnected()
		{
			//occurs when Client loses the server connection.
			Console.WriteLine("Disconnected");
		}

		protected override void OnDisableAndDisconnect()
		{
			//occurs when Server force closes the connection. Auto reconnection is stopped.
			//class needs to be invoked again as new.
		}

		//called by the thread calls HandleResponse function
		//thread safe if calling thread is the main thread
		protected override void OnEvent(byte OpCode, byte[] data)
		{
			switch (OpCode)
			{
				case 1:
					Console.WriteLine("Event 1");
					MemoryStream bytes = new MemoryStream(data);
					var strin = StringProxy.Deserialize(bytes);
					ByteProxy.DeserializeArray(bytes);
					var omg = StringProxy.Deserialize(bytes);
					Console.WriteLine(strin);
					Console.WriteLine(omg);
					break;
			}
		}

		public void SendEvent(byte OpCode, MemoryStream data)
		{
			Send(OpCode, data);
		}
	}
}
