using System;
using System.IO;
using System.Reflection;

namespace TroiletCore
{
    public static partial class Utils
    {
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
    }
}
