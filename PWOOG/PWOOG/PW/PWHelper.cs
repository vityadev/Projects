using System.Text;
using System.Security.Cryptography;

namespace PWOOG
{
    partial class PWClient
    {
        private static MD5 md5 = MD5.Create();

        public static byte[] GetHash(string login, string password, byte[] key)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(login + password);
            return new HMACMD5(md5.ComputeHash(bytes)).ComputeHash(key);
        }

        public static byte[] GetRC4Key(byte[] nHash, string login, byte[] hash)
        {
            return new HMACMD5(Encoding.ASCII.GetBytes(login)).ComputeHash(Helper.Assign(hash, nHash));
        }

        public static byte[] GetDecodeHash()
        {
            return Rand.NextBytes(16);
        }
    }
}
