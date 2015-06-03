using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PWOOG
{
    class Helper
    {
        public static byte[] GetArray(byte[] bytes, uint index, uint length)
        {
            byte[] array = new byte[length];
            for (int i = 0; i < length; ++i)
                array[i] = bytes[index + i];
            return array;
        }

        public static byte[] Assign(byte[] bytes, byte[] assignArray)
        {
            byte[] result = new byte[bytes.Length + assignArray.Length];
            for (int i = 0; i < bytes.Length; ++i)
                result[i] = bytes[i];
            for (int i = 0; i < assignArray.Length; ++i)
                result[i + bytes.Length] = assignArray[i];
            return result;
        }
    }
}
