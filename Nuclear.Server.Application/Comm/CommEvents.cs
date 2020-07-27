using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuclear.Server.Application.Comm
{
    public class CommEvents
    {
        public CommEvents(Action<byte[]> Sendevent)
        {
            SendEvent = Sendevent;
        }
        readonly Action<byte[]> SendEvent;

        public void SendUpdatedRoom(List<string> rooms)
        {
            //convert rooms to memory stream.
            //SendEvent(1, memorystream);
        }
    }
}
