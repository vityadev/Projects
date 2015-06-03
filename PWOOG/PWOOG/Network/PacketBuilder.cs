using System;
using System.Text;

namespace PWOOG
{
    class PacketBuilder
    {
        private byte[] PacketBytes { get; set; }
        private int PacketHeader { get; set; }
        public bool IsContainer { get; set; }

        public PacketBuilder(int data, bool isContainer)
        {
            this.PacketBytes = new byte[0];
            this.PacketHeader = data;
            this.IsContainer = isContainer;
        }

        public void Add(byte b)
        {
            byte[] array = new byte[this.PacketBytes.Length + 1];
            this.PacketBytes.CopyTo(array, 0);
            array[this.PacketBytes.Length] = b;
            this.PacketBytes = array;
        }

        public void Add(byte[] bytes)
        {
            byte[] array = new byte[this.PacketBytes.Length + bytes.Length];
            this.PacketBytes.CopyTo(array, 0);
            bytes.CopyTo(array, this.PacketBytes.Length);
            this.PacketBytes = array;
        }

        public void Add(UInt16 data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            if (!IsContainer)
                Array.Reverse(bytes);
            byte[] array = new byte[this.PacketBytes.Length + bytes.Length];
            this.PacketBytes.CopyTo(array, 0);
            bytes.CopyTo(array, this.PacketBytes.Length);
            this.PacketBytes = array;
        }

        public void Add(Int16 data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            if (!IsContainer)
                Array.Reverse(bytes);
            byte[] array = new byte[this.PacketBytes.Length + bytes.Length];
            this.PacketBytes.CopyTo(array, 0);
            bytes.CopyTo(array, this.PacketBytes.Length);
            this.PacketBytes = array;
        }

        public void Add(UInt32 data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            if (!IsContainer)
                Array.Reverse(bytes);
            byte[] array = new byte[this.PacketBytes.Length + bytes.Length];
            this.PacketBytes.CopyTo(array, 0);
            bytes.CopyTo(array, this.PacketBytes.Length);
            this.PacketBytes = array;
        }

        public void Add(Int32 data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            if (!IsContainer)
                Array.Reverse(bytes);
            byte[] array = new byte[this.PacketBytes.Length + bytes.Length];
            this.PacketBytes.CopyTo(array, 0);
            bytes.CopyTo(array, this.PacketBytes.Length);
            this.PacketBytes = array;
        }

        public void Add(UInt64 data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            if (!IsContainer)
                Array.Reverse(bytes);
            byte[] array = new byte[this.PacketBytes.Length + bytes.Length];
            this.PacketBytes.CopyTo(array, 0);
            bytes.CopyTo(array, this.PacketBytes.Length);
            this.PacketBytes = array;
        }

        public void Add(Int64 data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            if (!IsContainer)
                Array.Reverse(bytes);
            byte[] array = new byte[this.PacketBytes.Length + bytes.Length];
            this.PacketBytes.CopyTo(array, 0);
            bytes.CopyTo(array, this.PacketBytes.Length);
            this.PacketBytes = array;
        }

        public void Add(Single data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            if (!IsContainer)
                Array.Reverse(bytes);
            byte[] array = new byte[this.PacketBytes.Length + bytes.Length];
            this.PacketBytes.CopyTo(array, 0);
            bytes.CopyTo(array, this.PacketBytes.Length);
            this.PacketBytes = array;
        }

        public void AddCUInt(Int32 data)
        {
            byte[] bytes = CUInt.Pack(data);
            byte[] array = new byte[this.PacketBytes.Length + bytes.Length];
            this.PacketBytes.CopyTo(array, 0);
            bytes.CopyTo(array, this.PacketBytes.Length);
            this.PacketBytes = array;
        }

        public void AddUnicode(string str)
        {
            byte[] header = CUInt.Pack(str.Length * 2);
            byte[] buffer = Encoding.Unicode.GetBytes(str);
            byte[] array = new byte[this.PacketBytes.Length + buffer.Length + header.Length];
            this.PacketBytes.CopyTo(array, 0);
            header.CopyTo(array, this.PacketBytes.Length);
            buffer.CopyTo(array, this.PacketBytes.Length + header.Length);
            this.PacketBytes = array;
        }

        public void AddASCII(string str)
        {
            byte[] header = CUInt.Pack(str.Length);
            byte[] buffer = Encoding.ASCII.GetBytes(str);
            byte[] array = new byte[this.PacketBytes.Length + buffer.Length + header.Length];
            this.PacketBytes.CopyTo(array, 0);
            header.CopyTo(array, this.PacketBytes.Length);
            buffer.CopyTo(array, this.PacketBytes.Length + header.Length);
            this.PacketBytes = array;
        }

        public void AddUTF8(string str)
        {
            byte[] header = CUInt.Pack(str.Length);
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            byte[] array = new byte[this.PacketBytes.Length + buffer.Length + header.Length];
            this.PacketBytes.CopyTo(array, 0);
            header.CopyTo(array, this.PacketBytes.Length);
            buffer.CopyTo(array, this.PacketBytes.Length + header.Length);
            this.PacketBytes = array;
        }

        public void AddHex(string str)
        {
            byte[] header = CUInt.Pack(str.Length);
            byte[] buffer = HexParser.GetBytes(str);
            byte[] array = new byte[this.PacketBytes.Length + buffer.Length + header.Length];
            this.PacketBytes.CopyTo(array, 0);
            header.CopyTo(array, this.PacketBytes.Length);
            buffer.CopyTo(array, this.PacketBytes.Length + header.Length);
            this.PacketBytes = array;
        }

        public void AddHex(byte[] buffer)
        {
            byte[] header = CUInt.Pack(buffer.Length);
            byte[] array = new byte[this.PacketBytes.Length + buffer.Length + header.Length];
            this.PacketBytes.CopyTo(array, 0);
            header.CopyTo(array, this.PacketBytes.Length);
            buffer.CopyTo(array, this.PacketBytes.Length + header.Length);
            this.PacketBytes = array;
        }

        public void AddTime(DateTime i)
        {
            TimeSpan span = (TimeSpan)(i - new DateTime(0x7b2, 1, 1, 0, 0, 0, 0));
            this.Add((uint)span.TotalSeconds);
        }

        public byte[] Init()
        {
            byte[] header = CUInt.Pack(PacketHeader);
            byte[] packlength = CUInt.Pack(this.PacketBytes.Length);
            byte[] array = new byte[this.PacketBytes.Length + packlength.Length + header.Length];
            PacketBytes.CopyTo(array, packlength.Length + header.Length);
            packlength.CopyTo(array, header.Length);
            header.CopyTo(array, 0);
            return array;
        }
    }
}
