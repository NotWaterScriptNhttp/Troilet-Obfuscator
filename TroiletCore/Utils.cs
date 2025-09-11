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
    }
}
