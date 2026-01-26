using System;

using dnlib.DotNet;

using TroiletProt_DotNet.Enums;
using TroiletProt_DotNet.Attributes;

namespace TroiletProt_DotNet.Extensions
{
    [ProtectionLevel(ProtectionLevel.None, false)]
    public static class TypeRefExtensions
    {
        public static GenericInstSig ToGenSig(this TypeRef self, params TypeSig[] sigs)
        {
            return new GenericInstSig((ClassOrValueTypeSig)self.ToTypeSig(), sigs);
        }
    }
}
