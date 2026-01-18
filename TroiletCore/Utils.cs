using System;
using System.IO;
using System.Reflection;

namespace TroiletCore
{
    public static partial class Utils
    {
        public const ushort SizeNum = 1000; // kB

        public static Stream? GetResourceStream(string name)
        {
            Assembly asm = Assembly.GetCallingAssembly();
            foreach (var res in asm.GetManifestResourceNames())
                if (res.EndsWith($"Resources.{name}"))
                    return asm.GetManifestResourceStream(res);

            return null;
        }
        public static byte[]? GetResource(string name)
        {
            Assembly asm = Assembly.GetCallingAssembly();
            foreach (var res in asm.GetManifestResourceNames())
                if (res.EndsWith($"Resources.{name}"))
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Stream? s = asm.GetManifestResourceStream(res);
                        if (s == null)
                            return null;

                        s.CopyTo(ms);
                        return ms.ToArray();
                    }

            return null;
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
