using dnlib.DotNet;

namespace TroiletProt_DotNet
{
    public static class Globals
    {
        public const string ProtectionNS = "TProtections";

        public static TypeDef CreateType<T>() => new TypeDefUser(ProtectionNS, typeof(T).Name);
        public static MethodDef CreateMethod(string name, MethodSig sig) => new MethodDefUser(name, sig, MethodAttributes.Static | MethodAttributes.Public);
    }
}
