using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroiletProt_DotNet.Protections
{
    internal class Renamer : ProtectionBase
    {
        public class RenamerSession : ProtectionSession
        {
            internal int Types = 0;
            internal int Fields = 0;
            internal int Methods = 0;
            internal int Properties = 0;

            private Dictionary<string, string> _Namespaces = new();
            private Dictionary<string, string> _Methods = new();

            public RenamerSession(ModuleDef module) : base(module) {}

            public string GetNamespace(string ns)
            {
                if (_Namespaces.TryGetValue(ns, out var n))
                    return n;

                return _Namespaces[ns] = Globals.GetRandomString();
            }
            public string GetMethod(string m)
            {
                if (_Methods.TryGetValue(m, out var me))
                    return me;

                return _Methods[m] = Globals.GetRandomString();
            }

            public override ProtectionStatistics EndSession()
            {
                return new ProtectionStatistics()
                {
                    Message = "Renamed {0} types, {1} fields, {2} methods, {3} properties",
                    Params = new object[] { Types, Fields, Methods, Properties }
                };
            }
        }

        public override ProtectionSession StartSession(ModuleDef module)
        {
            RenamerSession s = new(module);

            void CheckType(TypeDef type, int depth = 0)
            {
                //TODO: Add exclusion checks, and CAttribute checks
                if (type != module.GlobalType)
                {
                    s.Types++;
                    type.Name = Globals.GetRandomString();
                    if (type.DeclaringType == null)
                        type.Namespace = s.GetNamespace(type.Namespace);
                }

                if (depth >= MAX_DEPTH)
                    return;

                foreach (TypeDef t in type.NestedTypes)
                    CheckType(t, depth + 1);

                foreach (FieldDef f in type.Fields)
                {
                    f.Name = Globals.GetRandomString();
                    s.Fields++;
                }
                foreach (PropertyDef p in type.Properties)
                {
                    p.Name = Globals.GetRandomString();
                    s.Properties++;
                }

                foreach (MethodDef m in type.Methods)
                {
                    s.Methods++;
                    m.Name = s.GetMethod(m.Name);
                    foreach (Parameter p in m.Parameters)
                        p.Name = null;
                }
            }

            foreach (TypeDef t in module.Types)
                CheckType(t);

            return s;
        }
    }
}
