using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using TroiletProt_DotNet.Extensions;

namespace TroiletProt_DotNet.Protections
{
    internal class ConstantProtection : ProtectionBase
    {
        public class ConstantSession : ProtectionSession
        {
            public ConstantSession(ModuleDef mdl) => Module = mdl;

            internal TypeDef? Type = null;
            internal uint Val = 0;
            internal Dictionary<string, string> Cache = new Dictionary<string, string>();

            internal string ProtectString(string s)
            {
                if (Cache.ContainsKey(s))
                    return Cache[s];

                string res = "";
                foreach (char c in s)
                    res += (char)(c ^ (char)0x6969);

                Cache[s] = res;
                return res;
            }
            private string UnprotectString(string s)
            {
                string res = "";
                for (int i = 0; i < s.Length; i++)
                    res.Insert(i, ((char)(s[i] ^ (char)0x6969)).ToString());

                return res;
            }


            public override ProtectionStatistics EndSession()
            {
                if (Val > 0)
                    Module.Types.Add(Type);

                return new ProtectionStatistics()
                {
                    Message = "Protected {0} constants",
                    Params = new object[] { Val }
                };
            }
        }

        public override ProtectionSession StartSession(ModuleDef module)
        {
            ConstantSession s = new ConstantSession(module);
            s.Type = Globals.CreateType<ConstantProtection>();

            ICorLibTypes types = module.CorLibTypes;
            MethodDef smeth = Globals.CreateMethod("UnprotectString", new MethodSig(CallingConvention.Default, 1, types.String, types.String));
            {
                CilBody body = smeth.Body = new CilBody();

                IMethod getChars = module.Import<String>("get_Chars");
                IMethod ctoString = module.Import<Char>("ToString");
                IMethod insert = module.Import<String>("Insert", new Type[] { typeof(string), typeof(int), typeof(string) });
                IMethod sLen = module.Import<String>("get_Length");

                body.Variables.Add(new Local(types.String));
                body.Variables.Add(new Local(types.Int32));
                body.Variables.Add(new Local(types.Char));

                body.Instructions.Add(new Instruction(OpCodes.Ldstr, ""));
                body.Instructions.Add(new Instruction(OpCodes.Stloc_0));

                body.Instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
                body.Instructions.Add(new Instruction(OpCodes.Stloc_1));

                body.Instructions.Add(new Instruction(OpCodes.Br, new InstrIdx(0)));

                body.Instructions.Add(new Instruction(OpCodes.Ldloc_0));
                body.Instructions.Add(new Instruction(OpCodes.Ldloc_1));

                body.Instructions.Add(new Instruction(OpCodes.Ldarg_0));
                body.Instructions.Add(new Instruction(OpCodes.Ldloc_1));
                body.Instructions.Add(new Instruction(OpCodes.Callvirt, getChars));
                body.Instructions.Add(new Instruction(OpCodes.Ldc_I4, 0x6969));
                body.Instructions.Add(new Instruction(OpCodes.Xor));
                body.Instructions.Add(new Instruction(OpCodes.Conv_U2));
                body.Instructions.Add(new Instruction(OpCodes.Stloc_2));

                body.Instructions.Add(new Instruction(OpCodes.Ldloca, 2));
                body.Instructions.Add(new Instruction(OpCodes.Call, ctoString));
                body.Instructions.Add(new Instruction(OpCodes.Callvirt, insert));
                body.Instructions.Add(new Instruction(OpCodes.Pop));

                body.Instructions.Add(new Instruction(OpCodes.Ldloc_1));
                body.Instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
                body.Instructions.Add(new Instruction(OpCodes.Add));
                body.Instructions.Add(new Instruction(OpCodes.Stloc_1));

                body.Instructions.Add(new Instruction(OpCodes.Ldloc_1));
                body.Instructions.Add(new Instruction(OpCodes.Ldarg_0));
                body.Instructions.Add(new Instruction(OpCodes.Callvirt, sLen));
                body.Instructions.Add(new Instruction(OpCodes.Clt));
                body.Instructions.Add(new Instruction(OpCodes.Brtrue, new InstrIdx(0)));
                
                body.Instructions.Add(new Instruction(OpCodes.Ldloc_0));
                //Not full

                body.Instructions.Add(new Instruction(OpCodes.Ldarg_0));
                body.Instructions.Add(new Instruction(OpCodes.Ret));
                body.Instructions.ResolveIndexes();
            }

            s.Type.Methods.Add(smeth);

            void CheckType(TypeDef type, int depth = 0)
            {
                if (depth >= 10)
                    return;

                foreach (TypeDef t in type.NestedTypes)
                    CheckType(t, depth + 1);

                foreach (MethodDef m in type.Methods)
                    if (m.HasBody)
                    {
                        CilBody body = m.Body;
                        body.SimplifyMacros(m.Parameters);

                        for (int i = 0; i < body.Instructions.Count; i++)
                        {
                            Instruction inst = body.Instructions[i];
                            switch (inst.OpCode.Code)
                            {
                                case Code.Ldstr:
                                    string str = (string)inst.Operand;

                                    inst.Operand = str;
                                    body.Instructions.Insert(++i, new Instruction(OpCodes.Call, smeth));

                                    s.Val++;
                                    break;

                                default:
                                    continue;
                            }
                        }

                        body.UpdateInstructionOffsets();
                        body.OptimizeBranches();
                    }
            }

            foreach (TypeDef t in module.Types)
                CheckType(t);

            return s;
        }
    }
}
