using System.IO;

namespace Nuclear.Common.Proxy
{
    public static class ByteProxy
	{
		public static byte Deserialize(Stream bytes)
		{
			return (byte)bytes.ReadByte();
		}

		public static void Serialize(Stream bytes, byte instance)
		{
			bytes.WriteByte(instance);
		}

        public static byte[] DeserializeArray(Stream bytes)
        {
            int length = Int32Proxy.Deserialize(bytes);
            byte[] buffer = new byte[length];
            bytes.Read(buffer, 0, length);
            return buffer;
        }

        public static void SerializeArray(Stream bytes,byte[] instance)
        {
            Int32Proxy.Serialize(bytes, instance.Length);
            bytes.Write(instance, 0, instance.Length);
        }
    }
}
