using System;
using System.Collections.Generic;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

using TroiletProt_DotNet.Extensions;

namespace TroiletProt_DotNet.Protections
{
    internal class TypeSpooferProtection : ProtectionBase
    {
        public class SpooferSession : ProtectionSession
        {
            private Dictionary<ITypeDefOrRef, TypeDef> _SpoofedTypes = new();
            private ICorLibTypes _Types;

            public SpooferSession(ModuleDef module) : base(module)
            {
                _Types = module.CorLibTypes;
            }

            public void SpoofBaseType(TypeDef type)
            {
                ITypeDefOrRef? tdor = type.BaseType;
                if (tdor == null) // Nothing to spoof
                    return;

                // Skip derivations of System.XXX types, as most of the time it breaks the assembly
                string fullname = tdor.FullName;
                if (fullname.IndexOf('.') == fullname.LastIndexOf('.') && fullname.StartsWith("System."))
                    return;

                if (type.IsEnum || type.IsValueType || tdor.IsValueType)
                    return;

                List<MethodDef> overrides = new();
                foreach (var m in type.Methods) 
                    if (m.IsReuseSlot && m.IsVirtual) // Check if the method is an override
                        overrides.Add(m);

                
                if (!_SpoofedTypes.TryGetValue(type.BaseType, out var spoofed))
                {
                    spoofed = new TypeDefUser(tdor.Name + "_Spoofed", tdor);
                    spoofed.Attributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;

                    var ctor = new MethodDefUser(".ctor", new MethodSig(CallingConvention.Default, 0, _Types.Void));
                    ctor.Attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig;
                    (ctor.Body = new CilBody()).Instructions.Add(new Instruction(OpCodes.Ret));
                    spoofed.Methods.Add(ctor);

                    foreach (var m in overrides)
                    {
                        var mc = new MethodDefUser(m.Name, m.MethodSig, m.ImplAttributes, m.Attributes);
                        var mcB = new MethodBuilder(mc);

                        if (mc.HasReturnType)
                        {
                            if (mc.ReturnType.IsValueType)
                            {
                                mcB.AddLocal(mc.ReturnType);
                                mcB.AddRefLocal(OpCodes.Ldloca_S, 0);
                                mcB.AddInst(OpCodes.Initobj, mc.ReturnType.ToTypeDefOrRef());
                                mcB.AddInst(OpCodes.Ldloc_0);
                            }
                            else mcB.AddInst(OpCodes.Ldnull);
                        }

                        mcB.AddInst(OpCodes.Ret);
                        spoofed.Methods.Add(mcB.Get());
                    }

                    _SpoofedTypes[tdor] = spoofed;
                    goto SET_BASETYPE;
                }

                // We can remove methods that aren't referenced, but are from the same basetype, as that means that its an override of a virtual function, and not an abstract one
                // The only solution to this, is doing it blindly, as we don't want to load referenced modules, and it should be safe, as we are only keeping the methods that are getting overriden in every child class
                IList<MethodDef> smethods = spoofed.Methods;
                foreach (var m in smethods)
                {
                    if (m.IsInstanceConstructor || m.IsStaticConstructor)
                        continue;

                    bool found = false;
                    foreach (var om in overrides)
                        if (m.Name == om.Name && m.MethodSig.IsSame(om.MethodSig))
                        {
                            found = true; 
                            break;
                        }

                    if (!found)
                        spoofed.Methods.Remove(m); // Remove the method
                }

            SET_BASETYPE:
                type.BaseType = spoofed;
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

            s.SpoofBaseType(type);
        }
    }
}
