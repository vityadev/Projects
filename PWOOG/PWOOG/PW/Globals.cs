using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PWOOG
{
    class Globals
    {
        public static byte[] RC4Client { get; set; }
        public static byte[] RC4Server { get; set; }

        public static string Login { get; set; }
        public static string Password { get; set; }

        public static byte[] Key { get; set; }
        public static byte[] Hash { get; set; }
        public static byte[] EncHash { get; set; }
        public static byte[] DecHash { get; set; }
        public static uint AccountId { get; set; }
    }
}
