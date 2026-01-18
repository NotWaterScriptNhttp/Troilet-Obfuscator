using System;

using dnlib.DotNet;

namespace TroiletProt_DotNet.Extensions
{
    internal static class SigExtensions
    {
        public static bool IsSame(this TypeSig sig, TypeSig sig2)
        {
            if (sig.ElementType != sig2.ElementType)
                return false;

            return sig.FullName == sig2.FullName;
        }
        public static bool IsSame(this MethodSig sig, MethodSig sig2)
        {
            if (!sig.RetType.IsSame(sig2.RetType))
                return false;
            if (sig.Params.Count != sig2.Params.Count)
                return false;

            for (int i = 0; i < sig.Params.Count; i++)
                if (!sig.Params[i].IsSame(sig2.Params[i]))
                    return false;

            if (sig.ContainsGenericParameter != sig2.ContainsGenericParameter)
                return false;
            if (sig.CallingConvention != sig2.CallingConvention)
                return false;

            return true;
        }
    }
}
