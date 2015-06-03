using System.Collections.Generic;

namespace PWOOG
{
    class PacketContainer
    {
        private byte HeadContainer { get; set; }
        private List<PacketBuilder> Packets;

        public PacketContainer(PacketBuilder packet, byte header)
        {
            this.HeadContainer = header;
            this.Packets = new List<PacketBuilder>();
            Add(packet);
        }

        public PacketContainer(List<PacketBuilder> packets)
        {
            this.Packets = packets;
        }

        public void Add(PacketBuilder packet)
        {
            this.Packets.Add(packet);
        }

        public byte[] InitContainer
        {
            get
            {
                List<byte> container = new List<byte>();
                foreach (var elm in Packets)
                    container.AddRange(elm.Init());

                container.InsertRange(0, (CUInt.Pack(container.Count)));
                container.Insert(0, HeadContainer);
                return container.ToArray();
            }
        }
    }
}
