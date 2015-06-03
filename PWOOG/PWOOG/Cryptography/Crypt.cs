using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace PWOOG
{
    class Crypt
    {
        private RC4 Encode; 
        private RC4 Decode;
        private MPPC Unpack = new MPPC();

        public Crypt(byte[] RC4Encode, byte[] RC4Decode)
        {
            Encode = new RC4();
            Decode = new RC4();

            Encode.Shuffle(RC4Encode);
            Decode.Shuffle(RC4Decode);
        }
        public void Encrypt(ref byte[] packet)
        {
            for (int i = 0; i < packet.Length; i++)
                packet[i] = Encode.Encode(packet[i]);
        }
        public void Decrypt(ref byte[] packet)
        {
            for (int i = 0; i < packet.Length; i++)
                packet[i] = Decode.Encode(packet[i]);

            using (MemoryStream ms = new MemoryStream())
            {
                foreach (byte b in packet)
                {
                    byte[] r = Unpack.AddByte(b).ToArray();
                    ms.Write(r, 0, r.Length);
                }
                packet = ms.ToArray();
            }
        }
    }
}
