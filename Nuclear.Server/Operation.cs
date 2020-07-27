using Nuclear.Common;
using Nuclear.Common.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nuclear.Server
{
    internal class Operation
    {
        internal Operation(Peer peer,Packet packet)
        {
            Peer = peer;
            MemoryStream stream = new MemoryStream(packet.Data);
            ByteProxy.Deserialize(stream);
            OpCode = ByteProxy.Deserialize(stream);
            Data = stream;
        }

        internal void Dispose()
        {
            Data.Dispose();
        }

        internal Peer Peer { get; set; }
        internal byte OpCode { get; set; }
        internal MemoryStream Data { get; set; }
    }
}
