using System;

namespace TroiletProt_DotNet.Enums
{
    [Flags]
    public enum ProtectionLevel : int
    {
        None = 0,
        Constants = 1,
        Code = 2, // Has no effect in types
        Name = 4,

        Full = Constants | Code | Name
    }
}
