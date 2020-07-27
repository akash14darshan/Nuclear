using System;

namespace Nuclear.Server.Application.Comm
{
    public class CommPeer
    {
        public CommPeer(Peer peer,string authToken)
        {
            Peer = peer;
            Event = new CommEvents(peer.SendEvent);
            GetUser(authToken);
        }
        public Peer Peer { get; set; }
        public CommEvents Event { get; set; }
        public string AuthToken { get; set; }
        public string Name { get; set; }
        public void GetUser(string authToken)
        {
            var result = "placeholder";
            if (result == null)
                throw new ArgumentNullException("Could not retrieve user with Authtoken " + authToken);
            AuthToken = authToken;
        }
    }
}
