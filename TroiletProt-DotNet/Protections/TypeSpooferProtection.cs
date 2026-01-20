using System;
using System.Collections.Generic;

using dnlib.DotNet;

using TroiletProt_DotNet.Extensions;

namespace TroiletProt_DotNet.Protections
{
    internal class TypeSpooferProtection : ProtectionBase
    {
        public class SpooferSession : ProtectionSession
        {
            private Dictionary<TypeDef, TypeDef> _SpoofedTypes = new();

            public SpooferSession(ModuleDef module) : base(module) {}

            public TypeSig Spoof(TypeSig ts)
            {
                return ts;
            }
            public ITypeDefOrRef? Spoof(ITypeDefOrRef? type)
            {
                if (type == null || type.IsTypeRef || ((TypeDef)type).IsSealed)
                    return type;

                TypeDef td = (TypeDef)type;
                if (_SpoofedTypes.TryGetValue(td, out var t))
                    return t;

                var tdu = new TypeDefUser("SpoofedTypes", "Spoofed_" + td.Name, type);
                tdu.Attributes = td.Attributes; // Remove abstract
                _SpoofedTypes[td] = tdu;

                return tdu;
            }

            public override ProtectionStatistics EndSession()
            {
                foreach (var kvp in _SpoofedTypes)
                    Module.Types.Add(kvp.Value);

                return new ProtectionStatistics()
                {
                    Message = "{0} types",
                    Params = new object[] { _SpoofedTypes.Count }
                };
            }
        }

        public override string Name { get; protected set; } = "TypeSpoofer";

        public override bool CanProtect() => PluginConfig.Instance.GetValueBool("Protections", "spooftypes_prot", false);
        public override ProtectionSession StartSession(ModuleDef module) => new SpooferSession(module);

        public override void OnType(ProtectionSession session, TypeDef type)
        {
            SpooferSession s = (SpooferSession)session;

            type.BaseType = s.Spoof(type.BaseType);
        }
    }
}
