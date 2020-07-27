using Nuclear.Server.Application.Comm;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Nuclear.Server.Application.Room
{
    public class LobbyRoom
    {
        public void OnJoinRoom(Peer peer,string authToken)
        {
            CommPeer commpeer = new CommPeer(peer, authToken);
            CommPeers[peer] = commpeer;
        }

        public void OnLeaveRoom(Peer peer)
        {
            CommPeers.TryRemove(peer, out _);
        }

        public void Tick()
        {
            ParallelTick(); //heavy operations.
            SerialTick(); //light operations, and operations that involve sending data to client.
        }

        private void SerialTick() //sending operation to client
        {
            foreach(var peer in CommPeers)
            {
                //task here
            }
        }

        private void ParallelTick()
        {
            Parallel.ForEach(CommPeers, new ParallelOptions { MaxDegreeOfParallelism = 10000 }, CommPeers =>
            {
                //task here
            });
        }

        readonly ConcurrentDictionary<Peer,CommPeer> CommPeers = new ConcurrentDictionary<Peer, CommPeer>();
    }
}
