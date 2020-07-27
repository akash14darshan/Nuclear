using Nuclear.Common;
using Nuclear.Common.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Nuclear.Server
{
	public class Peer : IDisposable
	{
		public Peer(IPEndPoint endPoint)
		{
			EndPoint = endPoint;
			lastUpdateTime = DateTime.Now;
		}
		public IPEndPoint EndPoint;
		public bool HasError = false;
		internal bool IsAlive()
		{
			return (DateTime.Now - lastUpdateTime).TotalSeconds < DisconnectionTimeInSecs;
		}

		internal void SetEventSender(EventSender Sender)
		{
			DataSender = Sender;
		}

		public void Dispose()
		{
			DisableAndDisconnect();
		}

		internal void RenewLastUpdate()
		{
			lastUpdateTime = DateTime.Now;
		}

		private void DisableAndDisconnect()
		{
			HasError = true;
			SendRaw(new byte[] { 0xFE });
		}

		internal void SendRaw(byte[] data)
		{
			DataSender.Enqueue(new Packet(EndPoint, data));
		}

		public void SendEvent(byte[] data)
		{
			if(data.Length <= 60000)
			{
				byte[] ToSend = new byte[data.Length + 1];
				ToSend[0] = 0xFD;
				data.CopyTo(ToSend, 1);
				SendRaw(ToSend);
			}
			else
			{
				foreach(var part in Slices(data,60000))
				{
					SendRaw(part);
				}
			}
		}

		private byte[] CopySlice(byte[] source, int index, int length,bool islast)
		{
			int n = length;
			byte[] slice = null;

			if (source.Length < index + length)
			{
				n = source.Length - index;
			}

			if (slice == null) slice = new byte[n+1];
			Array.Copy(source, index, slice, 1, n);
			slice[0] = islast ? (byte)0xFB : (byte)0xFC;
			return slice;
		}

		private IEnumerable<byte[]> Slices(byte[] source, int size)
		{
			for (var i = 0; i < source.Length; i += size)
			{
				yield return CopySlice(source, i, size, source.Length - i < 60000);
			}
		}

		private EventSender DataSender;
		readonly int DisconnectionTimeInSecs = 5;
		DateTime lastUpdateTime;
	}
}
