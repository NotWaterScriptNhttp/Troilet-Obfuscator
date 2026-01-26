using System;

using dnlib.DotNet;

namespace TroiletProt_DotNet.Extensions
{
    internal static class TypeDefExtensions
    {
        public static FieldDef AddField(this TypeDef self, string name, TypeSig type, FieldAttributes attrs = FieldAttributes.Private | FieldAttributes.Static)
        {
            var fld = new FieldDefUser(name, new FieldSig(type), attrs);
            self.Fields.Add(fld);

            return fld;
        }
    }
}
