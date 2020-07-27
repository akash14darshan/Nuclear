using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Nuclear.Common
{
	public abstract class UDP
	{
		protected UDP()
		{
			_client = new UdpClient();
		}

		protected void RenewClient()
		{
			_client.Close();
			_client = new UdpClient();
		}

		protected async Task<Packet> ReceiveAsync()
		{
			var result = await _client.ReceiveAsync();
			return new Packet(result.RemoteEndPoint, result.Buffer);
		}

		protected Packet Receive(ref IPEndPoint end)
		{
			var result = _client.Receive(ref end);
			return new Packet(ref end, result);
		}

		public virtual void Dispose()
		{
			_client.Close();
			Console.WriteLine("Dispose from UDP");
		}

		protected UdpClient _client;
	}
}
