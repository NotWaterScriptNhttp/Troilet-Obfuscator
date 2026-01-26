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
        public static IMethod? ImportMethod(this ModuleDef md, TypeSpec ts, string name, Type[]? types = null)
        {
            var methods = new List<MethodDef>();
            var td = ts.ResolveTypeDef();
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
            return md.Import(new MemberRefUser(md, name, meth.MethodSig, ts));
        }
        public static MemberRef? ImportField(this ModuleDef md, TypeSpec ts, string name)
        {
            var td = ts.ResolveTypeDef();
            foreach (var f in td.Fields)
                if (f.Name == name)
                    return md.Import(new MemberRefUser(md, name, f.FieldSig, td));

            return null;
        }

        public static IMethod? ImportMethod(this ModuleDef md, Type type, string name, Type[]? types = null) => md.ImportMethod(RefResolver.GetType(type), name, types);
        public static IMethod? ImportMethod<T>(this ModuleDef md, string name, Type[]? types = null) => md.ImportMethod(typeof(T), name, types);

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
