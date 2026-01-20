using System;
using System.IO;
using System.Reflection;

namespace TroiletCore
{
    public static partial class Utils
    {
        public const ushort SizeNum = 1000; // kB

        private static Stream? _GetResStream(Assembly asm, string name)
        {
            string? n = asm.GetName().Name;
            if (n == null)
                n = "";
            else n = n.Replace('-', '_').ToLower() + ".";

            foreach (var res in asm.GetManifestResourceNames())
                if (res.ToLower() == (n + name.ToLower()))
                    return asm.GetManifestResourceStream(res);

            return null;
        }

        public static Stream? GetResourceStream(string name) => _GetResStream(Assembly.GetCallingAssembly(), name);
        public static byte[]? GetResource(string name)
        {
            Stream? s = _GetResStream(Assembly.GetCallingAssembly(), name);
            if (s == null)
                return null;

            using (var ms = new MemoryStream())
            {
                s.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static bool CheckBytes(byte[] bytes, int blen, string data)
        {
            if (blen > bytes.Length)
                return false;

            for (int i = 0; i < blen; i++)
            {
                char c = data[i];
                if (c == '?')
                    continue;
                if (bytes[i] != (byte)c)
                    return false;
            }

            return true;
        }
        public static string ToSize(long size)
        {
            if (size <= 0)
                return "Empty";

            byte idx = 0;
            double val = size;
            while (val > SizeNum && idx < 3)
            {
                val /= SizeNum;
                idx++;
            }

            string sig = "bytes";
            if (idx > 0)
            {
                if (idx == 1)
                    sig = "kB";
                else if (idx == 2)
                    sig = "MB";
                else if (idx == 3)
                    sig = "GB";
                else sig = "?";
            }

            return $"{Math.Round(val, 2)} {sig}";
        }
    }
}
