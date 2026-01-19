using System;

namespace TroiletProt_DotNet.Extensions
{
    internal static class ByteExtensions
    {
        public static string ToB64(this byte[] data) => Convert.ToBase64String(data).Replace('+', '_');
        public static byte[] FromB64(this string str) => Convert.FromBase64String(str.Replace('_', '+'));

        public static string ToWide(this byte[] data)
        {
            string res = "";
            for (int i = 0; i < data.Length; i++)
            {
                if ((data.Length - i) <= 2)
                    res += (char)(data[i] << 8);
                else res += (char)((data[i] << 8) | data[++i]);
            }

            return res;
        }
    }
}
