using System;
using System.Collections.Generic;

using dnlib.DotNet;

using TroiletCore;
using TroiletProt_DotNet.Enums;
using TroiletProt_DotNet.Attributes;

namespace TroiletProt_DotNet
{
    public static class Globals
    {
        public const string ProtectionNS = "TProtections";

        private static Dictionary<object, ProtectionLevel> _LevelCache = new();

        public static Random Rand = new Random();
        public static MethodBuilder CCtor;

        public static byte[] Key = new byte[0];
        public static byte[] Salt = new byte[0];

        [EmbedReader]
        internal static byte[]? ReadEmbed(string name) => Utils.GetResource(name);

        public static TypeDef CreateType<T>(ModuleDef mdl)
        {
            TypeDef t = new TypeDefUser(ProtectionNS, typeof(T).Name, mdl.CorLibTypes.Object.TypeDefOrRef);
            t.Attributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;

            return t;
        }

        public static ProtectionLevel GetLevel(IHasCustomAttribute? attr, ProtectionLevel defValue = ProtectionLevel.Full)
        {
            if (attr == null)
                return defValue;
            if (_LevelCache.TryGetValue(attr, out ProtectionLevel pl))
                return pl;
            if (!attr.HasCustomAttributes)
                return defValue;

            foreach (var a in attr.CustomAttributes)
            {
                if (ProtectionLevelAttribute.CheckAttribute(a))
                {
                    attr.CustomAttributes.Remove(a); // We can safely remove this attribute, as its already cached and doesn't need to be referenced

                    ProtectionLevel p = (ProtectionLevel)a.ConstructorArguments[0].Value;
                    if ((bool)a.ConstructorArguments[1].Value)
                        p = ProtectionLevel.Full & ~p;

                    return _LevelCache[attr] = p;
                }
            }

            return defValue;
        }

        public static void Clear()
        {
            _LevelCache.Clear();
            Array.Clear(Key);
            Array.Clear(Salt);
        }
    }
}
