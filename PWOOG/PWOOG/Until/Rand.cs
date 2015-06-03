using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PWOOG
{
    class Rand
    {
        private static Random rand = new Random();

        public static byte[] NextBytes(int count)
        {
            byte[] buf = new byte[count];
            rand.NextBytes(buf);
            return buf;
        }

        public static int NextInt(int a, int b)
        {
            return rand.Next(a, b);
        }
    }
}
