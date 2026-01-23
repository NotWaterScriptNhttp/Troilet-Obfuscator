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
                Dictionary<IMethod, List<Instruction>> ctormap = new();
                foreach (var m in type.Methods)
                    if (m.IsInstanceConstructor && !m.IsPrivate && m.HasBody)
                    {
                        for (int i = m.Body.Instructions.Count - 1; 0 < i; i--)
                        {
                            Instruction inst = m.Body.Instructions[i];
                            if (inst.OpCode.Code == Code.Call)
                            {
                                IMethodDefOrRef mdor = (IMethodDefOrRef)inst.Operand;
                                if (mdor.Name == ".ctor" && mdor.DeclaringType == tdor)
                                {
                                    if (ctormap.TryGetValue(mdor, out var list))
                                        list.Add(inst);
                                    else ctormap[mdor] = new() { inst };

                                    break;
                                }
                            }
                        }
                    } 
                    else if (m.IsReuseSlot && m.IsVirtual) // Check if the method is an override
                        overrides.Add(m);

                if (!_SpoofedTypes.TryGetValue(type.BaseType, out var spoofed))
                {
                    spoofed = new TypeDefUser(tdor.Name + "_Spoofed", tdor);
                    spoofed.Attributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;

                    // Add base ctors
                    foreach (var kvp in ctormap)
                    {
                        var ctor = new MethodDefUser(".ctor", kvp.Key.MethodSig);
                        ctor.Attributes = MethodAttributes.Public | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

                        var body = ctor.Body = new CilBody();

                        for (int i = 0; i < ctor.Parameters.Count; i++)
                            body.Instructions.Add(new Instruction(OpCodes.Ldarg, ctor.Parameters[i]));
                        body.Instructions.Add(new Instruction(OpCodes.Call, kvp.Key));
                        body.Instructions.Add(new Instruction(OpCodes.Ret));
                        body.OptimizeMacros();

                        spoofed.Methods.Add(ctor);
                    }

                    // Add override methods
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
                }

                // We can remove methods that aren't referenced, but are from the same basetype, as that means that its an override of a virtual function, and not an abstract one
                // The only solution to this, is doing it blindly, as we don't want to load referenced modules, and it should be safe, as we are only keeping the methods that are getting overriden in every child class
                IList<MethodDef> sctors = spoofed.Methods;
                foreach (var m in sctors)
                {
                    bool found = false;
                    if (m.IsInstanceConstructor)
                    {
                        foreach (var kvp in ctormap)
                            if (m.MethodSig.IsSame(kvp.Key.MethodSig))
                            {
                                foreach (var inst in kvp.Value)
                                    inst.Operand = m;

                                found = true;
                                break;
                            }
                    } 
                    else foreach (var om in overrides)
                        if (m.Name == om.Name && m.MethodSig.IsSame(om.MethodSig))
                        {
                            found = true;
                            break;
                        }

                    if (!found)
                        spoofed.Methods.Remove(m); // Remove the method, as it isn't abstract or isn't part of the baseclass's ctors
                }

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
