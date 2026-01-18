using System;

using dnlib.DotNet;

namespace TroiletProt_DotNet
{
    public static class Globals
    {
        public const string ProtectionNS = "TProtections";

        public static Random Rand = new Random();

        public static byte[] Key = new byte[0];
        public static byte[] Salt = new byte[0];

        public static TypeDef CreateType<T>(ModuleDef mdl)
        {
            TypeDef t = new TypeDefUser(ProtectionNS, typeof(T).Name, mdl.CorLibTypes.Object.TypeDefOrRef);
            t.Attributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;

            return t;
        }

        public static string GetRandomString(int min = 8, int max = 16)
        {
            int len = Rand.Next(min, max);
            byte[] data = new byte[len];
            Rand.NextBytes(data);

            return Convert.ToBase64String(data).Replace('+', '_');
        }
    }
}
