using System;

using TroiletProt_DotNet.Attributes;

namespace TroiletProt_DotNet.Enums
{
    [Flags]
    [ProtectionLevel(None, false)]
    public enum ProtectionLevel : int
    {
        None = 0,
        Constants = 1,
        Code = 2, // Has no effect on types
        Name = 4,

        Full = Constants | Code | Name
    }
}
