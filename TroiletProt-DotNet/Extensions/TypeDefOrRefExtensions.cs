using System;

using dnlib.DotNet;

namespace TroiletProt_DotNet.Extensions
{
    internal static class TypeDefOrRefExtensions
    {
        public static GenericInstSig ToGenSig(this ITypeDefOrRef self, params TypeSig[] sigs) => new GenericInstSig((ClassOrValueTypeSig)self.ToTypeSig(), sigs);

        public static TypeDef ResolveTypeDef(this TypeSpec ts)
        {
            var sig = ts.TypeSig;
            if (sig is GenericInstSig gis)
                if (gis.GenericType.IsTypeDef)
                    return gis.GenericType.TypeDef;
                else return RefResolver.Resolve(gis.GenericType.TypeRef);

            var tdor = sig.ToTypeDefOrRef();
            if (tdor.IsTypeDef)
                return (TypeDef)tdor;

            return RefResolver.Resolve((TypeRef)tdor);
        }

        public static FieldDef AddField(this TypeDef self, string name, TypeSig type, FieldAttributes attrs = FieldAttributes.Private | FieldAttributes.Static)
        {
            var fld = new FieldDefUser(name, new FieldSig(type), attrs);
            self.Fields.Add(fld);

            return fld;
        }
    }
}
