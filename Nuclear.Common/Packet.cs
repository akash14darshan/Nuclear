using System.Net;

namespace Nuclear.Common
{
	public class Packet
	{
		public Packet(IPEndPoint endpoint,byte[] data)
		{
			EndPoint = endpoint;
			Data = data;
		}
		public Packet(ref IPEndPoint endpoint,byte[] data)
		{
			EndPoint = endpoint;
			Data = data;
		}

		public IPEndPoint EndPoint;
		public byte[] Data;
	}
}
