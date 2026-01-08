using dnlib.DotNet;

namespace TroiletProt_DotNet
{
    public static class Globals
    {
        public const string ProtectionNS = "TProtections";

        public static TypeDef CreateType<T>(ModuleDef mdl)
        {
            TypeDef t = new TypeDefUser(ProtectionNS, typeof(T).Name, mdl.CorLibTypes.Object.TypeDefOrRef);
            t.Attributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;

            return t;
        }
    }
}
