using System;

using TroiletProt_DotNet.Enums;

namespace TroiletProt_DotNet.Attributes
{
    public class ProtectionLevelAttribute : Attribute
    {
        public ProtectionLevel Level { get; set; }

        public ProtectionLevelAttribute(ProtectionLevel level)
        {
            Level = level;
        }
    }
}
