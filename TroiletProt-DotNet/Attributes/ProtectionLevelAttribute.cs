using System;

using dnlib.DotNet;

using TroiletProt_DotNet.Enums;

namespace TroiletProt_DotNet.Attributes
{
    public class ProtectionLevelAttribute : Attribute
    {
        public ProtectionLevelAttribute(ProtectionLevel level, bool isExclusion = true) {}

        public static bool CheckAttribute(CustomAttribute ca) => ca.ConstructorArguments.Count >= 2 && ca.TypeFullName == typeof(ProtectionLevelAttribute).FullName;
    }
}
