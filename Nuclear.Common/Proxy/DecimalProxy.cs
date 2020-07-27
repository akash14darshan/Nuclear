using System.IO;

namespace Nuclear.Common.Proxy.Views
{
    public static class DecimalProxy
    {
        public static decimal Deserialize(Stream bytes)
        {
            int[] bits = {
                Int32Proxy.Deserialize(bytes),
                Int32Proxy.Deserialize(bytes),
                Int32Proxy.Deserialize(bytes),
                Int32Proxy.Deserialize(bytes)
            };
            return new decimal(bits);
        }

        public static void Serialize(Stream bytes, decimal instance)
        {
            int[] bits = decimal.GetBits(instance);
            Int32Proxy.Serialize(bytes, bits[0]);
            Int32Proxy.Serialize(bytes, bits[1]);
            Int32Proxy.Serialize(bytes, bits[2]);
            Int32Proxy.Serialize(bytes, bits[3]);
        }
    }
}
