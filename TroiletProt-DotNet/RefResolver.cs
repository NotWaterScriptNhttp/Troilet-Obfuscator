using System;
using System.IO;
using System.Collections.Generic;

using dnlib.DotNet;

using TroiletProt_DotNet.Enums;
using TroiletProt_DotNet.Attributes;
using TroiletProt_DotNet.Extensions;

namespace TroiletProt_DotNet
{
    [ProtectionLevel(ProtectionLevel.None, false)]
    public class RefResolver
    {
        private static Dictionary<string, List<TypeDef>> _namespaces = new();
        private static Dictionary<Type, TypeDef?> _cache = new();
        private static Dictionary<TypeRef, TypeDef> _refCache = new();

        public static void LoadAssemblies(string dir)
        {
            if (!Directory.Exists(dir))
                return;

            foreach (var lib in Directory.GetFiles(dir, "*.dll"))
            {
                try
                {
                    var mdl = ModuleDefMD.Load(lib);
                    foreach (var t in mdl.Types)
                    {
                        if (t == mdl.GlobalType)
                            continue;

                        string ns = t.Namespace?.ToLower() ?? "";
                        if (!_namespaces.TryGetValue(ns, out var types))
                        {
                            types = new List<TypeDef>();
                            _namespaces[ns] = types;
                        }

                        types.Add(t);
                    }
                } catch(Exception _)
                {
                    Console.WriteLine("Failed to load assembly: {0}", Path.GetFileNameWithoutExtension(lib));
                }
            }
        }

        public static TypeDef? ResolveTypeNull(string ns, string name)
        {
            if (!_namespaces.TryGetValue(ns.ToLower(), out var types))
                return null;

            name = name.ToLower();
            foreach (var t in types)
                if (t.Name.ToLower() == name)
                    return t;

            return null;
        }
        public static TypeDef? ResolveTypeNull<T>()
        {
            Type t = typeof(T);
            if (_cache.TryGetValue(t, out var type))
                return type;

            return _cache[t] = ResolveTypeNull(t.Namespace ?? "", t.Name);
        }

        public static TypeDef ResolveType(Type t)
        {
            var res = ResolveTypeNull(t.Namespace ?? "", t.Name);
            if (res == null)
                throw new Exception("Failed to find type");

            return res;
        }
        public static TypeDef ResolveType<T>() => ResolveType(typeof(T));

        public static TypeSpec GetType(Type t) => new TypeSpecUser(ResolveType(t).ToTypeSig());
        public static TypeSpec GetType<T>() => GetType(typeof(T));

        public static TypeSpec GetType(Type t, params TypeSig[] ts) => new TypeSpecUser(ResolveType(t).ToGenSig(ts));
        public static TypeSpec GetType<T>(params TypeSig[] ts) => GetType(typeof(T), ts);

        public static TypeDef Resolve(TypeRef tr)
        {
            if (_refCache.TryGetValue(tr, out var t))
                return t;

            if (_namespaces.TryGetValue(tr.Namespace.ToLower(), out var types))
                foreach (var type in types)
                    if (type.Name == tr.Name)
                        return type;

            throw new Exception("TypeRef isn't loaded inside RefResolver!");
        }

        public static void Clear()
        {
            _refCache.Clear();
            _cache.Clear();
            _namespaces.Clear();
        }
    }
}
