using System;
using System.Reflection;
using System.Collections.Generic;

using dnlib.DotNet;

using TroiletProt_DotNet.Enums;
using TroiletProt_DotNet.Attributes;

namespace TroiletProt_DotNet.Extensions
{
    [ProtectionLevel(ProtectionLevel.None, false)]
    public static class ModuleExtensions
    {
        /*
        public static MemberRef? ImportField(this ModuleDef md, TypeSpec ts, string name)
        {
            var td = ts.ResolveTypeDef();
            foreach (var f in td.Fields)
                if (f.Name == name)
                    return md.Import(new MemberRefUser(md, name, f.FieldSig, td));

            return null;
        }*/

        private static Dictionary<Type, TypeSpec> _cache = new();

        public static void ClearCache() => _cache.Clear();

        public static TypeSpec ImportType(this ModuleDef md, Type type)
        {
            if (_cache.TryGetValue(type, out var ts))
                return ts;

            var t = RefResolver.ResolveType(type);
            return _cache[type] = new TypeSpecUser(md.Import(new TypeRefUser(md, t.Namespace, t.Name, t.Module)).ToTypeSig());
        }
        public static TypeSpec ImportType(this ModuleDef md, Type type, params TypeSig[] ts)
        {
            var t = RefResolver.ResolveType(type);
            return new TypeSpecUser(md.Import(new TypeRefUser(md, t.Namespace, t.Name, t.Module)).ToGenSig(ts));
        }

        public static TypeSpec ImportType<T>(this ModuleDef md) => md.ImportType(typeof(T));
        public static TypeSpec ImportType<T>(this ModuleDef md, params TypeSig[] ts) => md.ImportType(typeof(T), ts);

        public static IMethod? ImportMethod(this ModuleDef md, TypeSpec type, string name, Type[]? types = null)
        {
            var methods = new List<MethodDef>();
            var td = type.ResolveTypeDef();
            foreach (var m in td.Methods)
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
            return md.Import(new MemberRefUser(md, name, meth.MethodSig, type));
        }
        public static IMethod? ImportMethod(this ModuleDef md, Type type, string name, Type[]? types = null) => md.ImportMethod(md.ImportType(type), name, types);
        public static IMethod? ImportMethod<T>(this ModuleDef md, string name, Type[]? types = null) => md.ImportMethod(md.ImportType<T>(), name, types);

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
