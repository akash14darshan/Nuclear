using Nuclear.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nuclear.Client
{
    internal class PartsManager
    {
        internal bool IsPartData(Packet packet)
        {
            return packet != null && packet.Data != null && (packet.Data[0] == 0xFC || packet.Data[0] == 0xFB);
        }

        internal bool HandlePart(Packet packet,out byte[] data)
        {
            data = null;
            byte code = packet.Data[0];
            byte[] ToStore = new byte[packet.Data.Length - 1];
            Array.Copy(packet.Data, 1, ToStore, 0, ToStore.Length);
            Parts.Add(ToStore);
            if (code == 0xFC)
            {
                return false;
            }
            else
            {
                byte[][] Matrix = Parts.ToArray();
                Parts.Clear();
                byte[] result = new byte[Matrix.Sum(a => a.Length)];
                int offset = 0;
                foreach (byte[] array in Matrix)
                {
                    Buffer.BlockCopy(array, 0, result, offset, array.Length);
                    offset += array.Length;
                }
                data = result;
                return true;
            }
        }
        private readonly List<byte[]> Parts = new List<byte[]>();
    }
}
