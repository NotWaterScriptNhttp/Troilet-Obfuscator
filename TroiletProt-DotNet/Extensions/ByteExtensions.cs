using System;

namespace TroiletProt_DotNet.Extensions
{
    internal static class ByteExtensions
    {
        public static string ToB64(this byte[] data) => Convert.ToBase64String(data).Replace('+', '_');
        public static byte[] FromB64(this string str) => Convert.FromBase64String(str.Replace('_', '+'));
    }
}
