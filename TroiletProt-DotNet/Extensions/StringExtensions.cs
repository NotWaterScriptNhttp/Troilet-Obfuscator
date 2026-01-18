using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using dnlib.DotNet;

namespace TroiletProt_DotNet.Extensions
{
    internal static class StringExtensions
    {
        public const int FACTORIT = 4;
        public const int KEYSIZE = 256;
        public const int BLOCKSIZE = 128;

        private static byte[] Protect(byte[] val, byte[]? factor = null)
        {
            byte[] key = Globals.Key;

            if (factor != null)
            {
                // Adding factor, so the key can be fully mutated
                int flen = factor.Length;
                for (int i = 0; i < flen * FACTORIT; i++)
                    key[i % key.Length] ^= factor[i % flen];
            }

            using (var ms = new MemoryStream())
            {
                using (var aes = Aes.Create())
                {
                    aes.KeySize = KEYSIZE;
                    aes.BlockSize = BLOCKSIZE;
                    aes.Mode = CipherMode.CBC;

                    using (var rfc = new Rfc2898DeriveBytes(key, Globals.Salt, 1000, HashAlgorithmName.SHA256))
                    {
                        aes.Key = rfc.GetBytes(KEYSIZE / 8);
                        aes.IV = rfc.GetBytes(BLOCKSIZE / 8);
                    }

                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        cs.Write(val, 0, val.Length);
                }

                return ms.ToArray();
            }
        }

        public static string SReverse(this string str)
        {
            string res = "";
            foreach (var c in str.Reverse())
                res += c;

            return res;
        }

        // # to indicate that the string is protected
        public static string Protect(this UTF8String str, UTF8String? factor = null) => "#" + Protect(str.Data, factor?.Data).ToB64().SReverse();
        public static string Protect(this string str, string? factor = null) => "#" + Protect(Encoding.UTF8.GetBytes(str), factor != null ? Encoding.UTF8.GetBytes(factor) : null).ToB64().SReverse();
    }
}
