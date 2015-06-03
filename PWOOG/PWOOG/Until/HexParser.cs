using System.Globalization;

namespace PWOOG
{
    class HexParser
    {
        public static byte[] GetBytes(string str)
        {
            str = str.Replace(" ", "");
            str = str.Replace("-", "");
            int num = str.Length / 2;
            byte[] buffer = new byte[num];
            for (int i = 0; i < num; ++i)
                buffer[i] = byte.Parse(str.Substring(i * 2, 2), NumberStyles.HexNumber);
            return buffer;
        }
    }
}
