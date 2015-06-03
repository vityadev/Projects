using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace PWOOG
{
    class NetStream
    {
        private MemoryStream ms;
        public ushort Header { get; private set; }
        public bool IsContainer { get; private set; }
        public long Position { get { return ms.Position; } }
        public long Length { get { return ms.Length; } }

        public static NetStream[] FromBuff(byte[] buff)
        {
            List<NetStream> result = new List<NetStream>();

            ushort length;
            for (int i = 0; i < buff.Length; i += length)
            {
                ushort header;
                int len = 4;
                int _len = buff.Length - i;
                if (_len < len)
                    len = _len;

                using (MemoryStream ms = new MemoryStream(buff, i, len))
                {
                    header = (ushort)CUInt.UnpackStream(ms);
                    length = (ushort)CUInt.UnpackStream(ms);
                    i += (int)ms.Position;
                }

                NetStream stream = new NetStream(buff, i, length);
                stream.Header = header;
                stream.IsContainer = false;
                result.Add(stream);
            }
            return result.ToArray();
        }

        public static NetStream[] FromContainer(NetStream c)
        {
            c.IsContainer = true;
            List<NetStream> result = new List<NetStream>();
            ushort length;
            while (c.Position < c.Length)
            {
                byte _header = c.ReadByte();
                if (_header != 0x22)
                    throw new ArgumentException();

                length = (ushort)c.ReadFullCUInt();
                if (length < 0x80)
                    length -= 1;
                else
                {
                    c.ReadBytes(2);
                    length -= 2;
                }

                ushort header = c.ReadUInt16();

                length -= 2;

                NetStream stream = new NetStream(c.ms, length);
                stream.Header = header;
                stream.IsContainer = true;
                result.Add(stream);
               // Console.WriteLine(BitConverter.ToString(stream.ReadBytes((uint)stream.Length)));
               // if (stream.ReadByte() == 0x01)
               //     Console.WriteLine(stream.Header);
            }
            return result.ToArray();
        }

        public NetStream(MemoryStream ms, int length)
        {
            byte[] pkt = new byte[length];
            ms.Read(pkt, 0, pkt.Length);
            this.ms = new MemoryStream(pkt, 0, pkt.Length);
        }

        public NetStream(byte[] bytes, int offset, int length)
        {
            this.ms = new MemoryStream(bytes, offset, length);
        }

        public byte ReadByte()
        {
            return (byte)ms.ReadByte();
        }

        public byte[] ReadBytes(uint count)
        {
            byte[] result = new byte[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadByte();
            return result;
        }

        public UInt16 ReadUInt16()
        {
            byte[] bytes = new byte[2];
            ms.Read(bytes, 0, bytes.Length);
            if (!IsContainer)
                Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public UInt16[] ReadUInt16(uint count)
        {
            ushort[] result = new ushort[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadUInt16();
            return result;
        }

        public Int16 ReadInt16()
        {
            byte[] bytes = new byte[2];
            ms.Read(bytes, 0, bytes.Length);
            if (!IsContainer)
                Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        public Int16[] ReadInt16(uint count)
        {
            short[] result = new short[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadInt16();
            return result;
        }

        public UInt32 ReadUInt32()
        {
            byte[] bytes = new byte[4];
            ms.Read(bytes, 0, bytes.Length);
            if (!IsContainer)
                Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public UInt32[] ReadUInt32(uint count)
        {
            uint[] result = new uint[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadUInt32();
            return result;
        }

        public Int32 ReadInt32()
        {
            byte[] bytes = new byte[4];
            ms.Read(bytes, 0, bytes.Length);
            if (!IsContainer)
                Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public Int32[] ReadInt32(uint count)
        {
            int[] result = new int[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadInt32();
            return result;
        }

        public UInt64 ReadUInt64()
        {
            byte[] bytes = new byte[8];
            ms.Read(bytes, 0, bytes.Length);
            if (!IsContainer)
                Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public UInt64[] ReadUInt64(uint count)
        {
            ulong[] result = new ulong[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadUInt64();
            return result;
        }

        public Int64 ReadInt64()
        {
            byte[] bytes = new byte[8];
            ms.Read(bytes, 0, bytes.Length);
            if (!IsContainer)
                Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        public Int64[] ReadInt64(uint count)
        {
            long[] result = new long[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadInt64();
            return result;
        }

        public Single ReadSingle()
        {
            byte[] bytes = new byte[4];
            ms.Read(bytes, 0, bytes.Length);
            if (!IsContainer)
                Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        public Single[] ReadSingle(uint count)
        {
            Single[] result = new Single[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadSingle();
            return result;
        }

        public string ReadUnicode()
        {
            byte head = ReadByte();
            uint strlen = Convert.ToUInt32(head);
            byte[] str = ReadBytes(strlen);
            return Encoding.Unicode.GetString(str);
        }

        public string ReadASCII()
        {
            byte head = ReadByte();
            uint strlen = Convert.ToUInt32(head);
            byte[] str = ReadBytes(strlen);
            return Encoding.ASCII.GetString(str);
        }

        public string ReadUTF8()
        {
            byte head = ReadByte();
            uint strlen = Convert.ToUInt32(head);
            byte[] str = ReadBytes(strlen);
            return Encoding.UTF8.GetString(str);
        }

        public uint ReadCUInt()
        {
            byte b1 = ReadByte();
            if (((b1 > 0x7f) ? 2 : 1) != 2)
                return CUInt.Unpack(new byte[] { b1 });
            else
            {
                byte b2 = ReadByte();
                return CUInt.Unpack(new byte[] { b1, b2 });
            }
        }

        public uint ReadFullCUInt()
        {
            byte[] bytes = ReadBytes(2);
            return CUInt.Unpack(bytes);
        }

        public byte[] ReadHex()
        {
            uint strlen = ReadCUInt();
            return ReadBytes(strlen);
        }
    }
}
