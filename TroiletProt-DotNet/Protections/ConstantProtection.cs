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
            private static string UnprotectString(string s)
            {
                string res = "";
                foreach (char c in s)
                    res += (char)(c ^ (char)0x6969);

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
            s.Type = Globals.CreateType<ConstantProtection>(module);

            ICorLibTypes types = module.CorLibTypes;
            MethodBuilder smeth = new MethodBuilder("UnprotectString", types.String, new TypeSig[] { types.String });
            {
                IMethod getChars = module.ImportMethod<string>("get_Chars");
                IMethod charToString = module.ImportMethod<char>("ToString", new Type[0]);
                IMethod concat = module.ImportMethod<string>("Concat", new Type[] { typeof(string), typeof(string) });
                IMethod getLength = module.ImportMethod<string>("get_Length");

                smeth.AddLocal(types.String); // Result
                smeth.AddLocal(types.Int32); // Index
                smeth.AddLocal(types.Char); // CurrentChar

                // Set variables to default
                body.Instructions.Add(OpCodes.Ldstr, ""); // 0
                body.Instructions.Add(OpCodes.Stloc_0); // 1
                body.Instructions.Add(OpCodes.Ldc_I4, 0); // 2
                body.Instructions.Add(OpCodes.Stloc_1); // 3

                // Check length
                body.Instructions.Add(OpCodes.Br, new InstrIdx(21)); // 4

                // Get current char
                body.Instructions.Add(OpCodes.Ldarg_0); // 5
                body.Instructions.Add(OpCodes.Ldloc_1); // 6
                body.Instructions.Add(OpCodes.Callvirt, getChars); // 7

                // Xor the current char
                body.Instructions.Add(OpCodes.Ldc_I4, 0x6969); // 8
                body.Instructions.Add(OpCodes.Xor); // 9
                body.Instructions.Add(OpCodes.Conv_U2); // 10
                body.Instructions.Add(OpCodes.Stloc_2); // 11

                // Concat current char with result
                body.Instructions.Add(OpCodes.Ldloc_0); // 12 str1
                body.Instructions.Add(OpCodes.Ldloca, new LocalIdx(2)); // 13
                body.Instructions.Add(OpCodes.Callvirt, charToString); // 14 str2
                body.Instructions.Add(OpCodes.Call, concat); // 15
                body.Instructions.Add(OpCodes.Stloc_0); // 16

                // Increment index
                body.Instructions.Add(OpCodes.Ldloc_1); // 17
                body.Instructions.Add(OpCodes.Ldc_I4, 1); // 18
                body.Instructions.Add(OpCodes.Add); // 19
                body.Instructions.Add(OpCodes.Stloc_1); // 20

                // Check index with length
                body.Instructions.Add(OpCodes.Ldloc_1); // 21
                body.Instructions.Add(OpCodes.Ldarg_0); // 22
                body.Instructions.Add(OpCodes.Callvirt, getLength); // 23
                body.Instructions.Add(OpCodes.Blt, new InstrIdx(5)); // 24

                // Return result
                body.Instructions.Add(OpCodes.Ldloc_0); // 25
                body.Instructions.Add(OpCodes.Ret); // 26
                body.Instructions.ResolveIndexes(body.Variables);
                body.Instructions.OptimizeMacros();
                body.Instructions.OptimizeBranches();
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

                                    inst.Operand = s.ProtectString(str);
                                    body.Instructions.Insert(++i, new Instruction(OpCodes.Call, smeth));

                                    s.Val++;
                                    break;

                                default:
                                    continue;
                            }
                        }

                        body.UpdateInstructionOffsets();
                        body.OptimizeMacros();
                        body.OptimizeBranches();
                    }
            }

            foreach (TypeDef t in module.Types)
                CheckType(t);

            return s;
        }
    }
}
