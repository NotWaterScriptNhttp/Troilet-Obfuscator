using System;
using System.Reflection;
using System.Collections.Generic;

using dnlib.DotNet;

namespace TroiletProt_DotNet.Extensions
{
    internal static class ModuleExtension
    {
        private const BindingFlags AllFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        public static TypeSig ImportAsSig<T>(this ModuleDef self) => self.ImportAsTypeSig(typeof(T));
        public static ITypeDefOrRef Import<T>(this ModuleDef self) => self.Import(typeof(T));

        public static IMethod ImportMethod(this ModuleDef self, Type type, string mName, Type[]? types = null)
        {
            MethodInfo? m;
            if (types != null)
                m = type.GetMethod(mName, AllFlags, types);
            else m = type.GetMethod(mName, AllFlags);

            return self.Import(m);
        }
        public static IMethod ImportMethod<T>(this ModuleDef self, string mName, Type[]? types = null) => self.ImportMethod(typeof(T), mName, types);
        public static IMethod ImportCtor<T>(this ModuleDef self, Type[] types) => self.Import(typeof(T).GetConstructor(AllFlags, types));

        public static MemberRef ImportField<T>(this ModuleDef self, string fName) => self.Import(typeof(T).GetField(fName, AllFlags));

        public static TypeDef? ExclusionToType(this ModuleDef mdl, Exclusion ex)
        {
            foreach (TypeDef t in mdl.Types)
                if (t.FullName == ex.Type)
                    return t;

            return null;
        }
    }
}
