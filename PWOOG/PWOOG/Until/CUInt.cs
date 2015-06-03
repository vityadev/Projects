using System;
using System.IO;

namespace PWOOG
{
    class CUInt
    {
        public static byte[] Pack(Int32 data)
        {
            byte[] array = new byte[1];
            if (data > 0x7f)
            {
                array = new byte[2];
                array = BitConverter.GetBytes((short)data);
                Array.Reverse(array);
                array[0] = (byte)(array[0] + 0x80);
                return array;
            }
            array[0] = (byte)data;
            return array;
        }

        public static uint Unpack(byte[] bytes)
        {
            if (bytes[0] > 0x7f)
                return BitConverter.ToUInt16(new byte[] { bytes[1], (byte)(bytes[0] - 0x80) }, 0);
            else
                return bytes[0];
        }

        public static uint UnpackStream(MemoryStream stream)
        {
            byte b1 = (byte)stream.ReadByte();
            if (b1 < 0x7f)
                return b1;
            else
            {
                b1 -= 0x80;
                byte b2 = (byte)stream.ReadByte();
                return BitConverter.ToUInt16(new byte[] { b2, b1 }, 0);
            }
        }
    }
}
