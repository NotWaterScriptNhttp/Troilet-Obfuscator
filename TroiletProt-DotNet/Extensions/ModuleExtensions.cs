using System;
using System.Reflection;
using System.Collections.Generic;

using dnlib.DotNet;

using TroiletProt_DotNet.Enums;
using TroiletProt_DotNet.Attributes;

namespace TroiletProt_DotNet.Extensions
{
    [ProtectionLevel(ProtectionLevel.None, false)]
    public static class ModuleExtension
    {
        private static TypeDef ResolveType(Type t)
        {
            var res = RefResolver.ResolveTypeNull(t.Namespace ?? "", t.Name);
            if (res == null)
                throw new Exception("Failed to find type");

            return res;
        }
        private static TypeDef ResolveType<T>() => ResolveType(typeof(T));

        public static TypeSig ImportAsSig(this ModuleDef self, Type t) => self.Import(ResolveType(t)).ToTypeSig();
        public static TypeSig ImportAsSig<T>(this ModuleDef self) => self.ImportAsSig(typeof(T));

        public static TypeSig ImportAsSig(this ModuleDef self, Type t, params TypeSig[] ts) => self.Import(ResolveType(t)).ToGenSig(ts);
        public static TypeSig ImportAsSig<T>(this ModuleDef self, params TypeSig[] ts) => self.ImportAsSig(typeof(T), ts);

        public static TypeRef Import(this ModuleDef self, Type t) => self.Import(ResolveType(t));
        public static TypeRef Import<T>(this ModuleDef self) => self.Import(ResolveType<T>());

        public static IMethod? ImportMethod(this ModuleDef self, Type type, string name, Type[]? types = null)
        {
            var t = ResolveType(type);

            var methods = new List<MethodDef>();
            foreach (var m in t.Methods)
                if (m.Name == name && (types == null || types.Length == m.MethodSig.Params.Count))
                {
                    if (types != null)
                    {
                        bool validSig = true;
                        var ps = m.MethodSig.Params;
                        for (int i = 0; i < ps.Count; i++)
                            if (types[i] != null && ps[i].FullName != types[i].FullName)
                            {
                                validSig = false;
                                break;
                            }

                        if (!validSig)
                            continue;
                    }

                    methods.Add(m);
                }

            if (methods.Count > 1)
                throw new Exception("Too many methods found!");

            if (methods.Count == 0)
                return null;

            var meth = methods[0];
            return self.Import(new MemberRefUser(meth.Module, name, meth.MethodSig, self.Import(t)));
        }
        public static IMethod? ImportMethod<T>(this ModuleDef self, string name, Type[]? types = null) => self.ImportMethod(typeof(T), name, types);

        public static MemberRef? ImportField(this ModuleDef self, Type type, string name)
        {
            var t = ResolveType(type);

            foreach (var f in t.Fields)
                if (f.Name == name)
                    return self.Import(new MemberRefUser(f.Module, name, f.FieldSig));

            return null;
        }
        public static MemberRef? ImportField<T>(this ModuleDef self, string name) => self.ImportField(typeof(T), name);

        /*
        public static TypeDef? ExclusionToType(this ModuleDef mdl, Exclusion ex)
        {
            foreach (TypeDef t in mdl.Types)
                if (t.FullName == ex.Type)
                    return t;

            return null;
        }*/
    }
}
