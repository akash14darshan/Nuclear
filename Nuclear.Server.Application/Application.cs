using Nuclear.Common.Proxy;
using System;
using System.IO;
using Nuclear.Server.Application.Room;
using System.Threading;

namespace Nuclear.Server.Application
{
    public class Application : NuclearServer
    {
        public LobbyRoom Room = new LobbyRoom();
        public Application(int port,int tickrate) : base(port,tickrate)
        {
            TestSpam();
            TestServer();
        }

        void TestSpam()
        {
            new Thread(() =>
            {
                int i = 0;
                while (true)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        i++;
                        ByteProxy.Serialize(stream, 1);
                        StringProxy.Serialize(stream, "Hello world"+ i);
                        ByteProxy.SerializeArray(stream, new byte[3000000]);
                        StringProxy.Serialize(stream, "over");
                        TestSendAll(stream.ToArray(),i);
                    }
                    Thread.Sleep(1000);
                }
            }).Start();
        }

        private void TestServer()
        {
            Console.WriteLine("All text written, and entered will be sent to connected clients here onwards");
            while (true)
            {
                var msgstr = Console.ReadLine();
                if (msgstr == "dc")
                {
                    DisconnectAll();
                    continue;
                }
                using (MemoryStream stream = new MemoryStream())
                {
                    ByteProxy.Serialize(stream, 1);
                    StringProxy.Serialize(stream, msgstr);
                    TestSendAll(stream.ToArray(),0);
                }
            }
        }

        public void DisconnectAll()
        {
            lock (PeerLock)
            {
                foreach (var peer in Peers)
                {
                    peer.Dispose();
                }
            }
        }

        protected override void OnPeerJoin(Peer peer)
        {
            Console.WriteLine(peer.EndPoint.ToString() + " has joined the server");
        }

        protected override void OnPeerLeave(Peer peer)
        {
            Console.WriteLine(peer.EndPoint.ToString() + " has left the server");
        }

        protected override void OnTick()
        {
            Room.Tick();
        }

        protected override void OnException(Exception e)
        {
            Console.WriteLine(e);
        }

        protected override void OnEvent(Peer peer, byte Opcode,MemoryStream stream)
        {
            switch(Opcode)
            {
                case 1:
                    string message = StringProxy.Deserialize(stream);
                    Console.WriteLine("Message from EndPoint: "+ peer.EndPoint.ToString() + " : " +message);
                    break;
            }
            
        }
    }
}
