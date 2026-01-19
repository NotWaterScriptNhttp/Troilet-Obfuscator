using System;
using System.Collections.Generic;

using dnlib.DotNet;

using TroiletProt_DotNet.Enums;
using TroiletProt_DotNet.Extensions;

namespace TroiletProt_DotNet.Protections
{
    internal class Renamer : ProtectionBase
    {
        public class RenamerSession : ProtectionSession
        {
            private Dictionary<UTF8String, string> _Namespaces = new();
            private Dictionary<UTF8String, Dictionary<TypeDef, List<MethodDef>>> _NameToBase = new(); // Very ugly

            internal Dictionary<TypeDef, bool> Checked = new();

            internal int Types = 0;
            internal int Fields = 0;
            internal int Methods = 0;
            internal int Properties = 0;
            internal int Events = 0;

            public RenamerSession(ModuleDef module) : base(module) {}

            public string GetNamespace(UTF8String ns)
            {
                if (_Namespaces.TryGetValue(ns, out var n))
                    return n;

                return _Namespaces[ns] = ns.Protect();
            }

            public void AddMethod(TypeDef t, MethodDef m)
            {
                List<MethodDef>? ids;
                if (!_NameToBase.TryGetValue(m.Name, out var idMap))
                {
                    ids = new List<MethodDef>();
                    idMap = new Dictionary<TypeDef, List<MethodDef>>();
                    idMap[t] = ids;
                    _NameToBase[m.Name] = idMap;
                }
                else if (!idMap.TryGetValue(t, out ids))
                {
                    ids = new List<MethodDef>();
                    idMap[t] = ids;
                }

                ids.Add(m);
            }

            public override ProtectionStatistics EndSession()
            {
                return new ProtectionStatistics()
                {
                    Message = "{0} types, {1} fields, {2} methods, {3} properties, {4} events",
                    Params = new object[] { Types, Fields, Methods, Properties, Events }
                };
            }
        }

        public override string Name { get; protected set; } = "Renamer";

        public override bool CanProtect() => true;
        public override ProtectionSession StartSession(ModuleDef module)
        {
            // Find all possible virtual/abstract/interface methods
            var s = new RenamerSession(module);

            //TODO: Use this data in OnType
            void CheckType(TypeDef t, int depth = 0)
            {
                if (depth >= 10)
                    return;

                foreach (TypeDef nt in t.NestedTypes)
                    CheckType(nt, depth + 1);

                foreach (var m in t.Methods)
                {
                    // Check abstract/virtual/interface
                    if (m.IsVirtual && m.IsNewSlot)
                        s.AddMethod(t, m);
                }
            }

            foreach (TypeDef t in module.Types)
                CheckType(t);

            return s;
        }

        public override void OnType(ProtectionSession session, TypeDef type)
        {
            RenamerSession s = (RenamerSession)session;
            if (s.Checked.ContainsKey(type))
                return;

            //TODO: Check exclusion
            bool isAttr = type.IsPublic && type.BaseType.FullName == "System.Attribute"; // Skip types that are public and are attributes
            bool rename = CheckLevel(type, ProtectionLevel.Name, isAttr ? ProtectionLevel.None : ProtectionLevel.Full);

            foreach (FieldDef f in type.Fields)
            {
                if (!CheckLevel(f, ProtectionLevel.Name, rename ? ProtectionLevel.Full : ProtectionLevel.None))
                    continue;

                f.Name = f.Name.Protect(type.Name);
                s.Fields++;
            }
            foreach (PropertyDef p in type.Properties)
            {
                if ((p.GetMethod != null && p.GetMethod.IsVirtual) || (p.SetMethod != null && p.SetMethod.IsVirtual))
                    continue;

                if (!CheckLevel(p, ProtectionLevel.Name, rename ? ProtectionLevel.Full : ProtectionLevel.None))
                    continue;

                p.Name = p.Name.Protect(type.Name);
                s.Properties++;
            }
            foreach (EventDef e in type.Events)
            {
                if (!CheckLevel(e, ProtectionLevel.Name, rename ? ProtectionLevel.Full : ProtectionLevel.None))
                    continue;

                e.Name = e.Name.Protect(type.Name);
                s.Events++;
            }

            Dictionary<UTF8String, string> mnames = new();
            foreach (MethodDef m in type.Methods)
            {
                if (m.IsInstanceConstructor || m.IsStaticConstructor)
                    continue;

                // Method has interface overrides
                if (m.HasOverrides)
                {
                    IMethodDefOrRef md = m.Overrides[0].MethodDeclaration;
                    if (md is MethodDef)
                    {
                        OnType(session, (TypeDef)md.DeclaringType); // Do renaming on type, before we actually use the new name
                        m.Name = md.DeclaringType.FullName + "." + md.Name;
                    }

                    continue;
                }

                // Skip this
                if (m.IsVirtual)
                    continue;

                // Method is abstract/virtual
                //bool isBase = m.IsNewSlot && !m.IsFinal && m.IsVirtual;

                if (!rename)
                {
                    // Static methods inside attributes, should be protected
                    if (!CheckLevel(m, ProtectionLevel.Name, (isAttr && m.IsStatic) ? ProtectionLevel.Full : ProtectionLevel.None))
                        continue;
                }
                else if (!CheckLevel(m, ProtectionLevel.Name, ProtectionLevel.Full))
                    continue;

                foreach (Parameter p in m.Parameters)
                    p.Name = null;

                UTF8String oname = m.Name;
                if (!mnames.TryGetValue(oname, out string? name))
                    name = mnames[oname] = oname.Protect(type.Name);

                m.Name = name;
            }

            if (rename)
            {
                type.Name = type.Name.Protect(type.Namespace);
                if (type.DeclaringType == null && !string.IsNullOrEmpty(type.Namespace))
                    type.Namespace = s.GetNamespace(type.Namespace);
                s.Types++;
            }
            s.Checked[type] = true;
        }
    }
}
