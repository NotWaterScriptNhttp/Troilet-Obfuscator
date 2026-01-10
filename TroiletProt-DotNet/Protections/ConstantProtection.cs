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
            internal TypeDef? Type = null;
            internal uint Val = 0;
            internal ushort Key = 0;
            internal Dictionary<string, string> Cache = new Dictionary<string, string>();

            public ConstantSession(ModuleDef mdl)
            {
                Module = mdl;
                Key = (ushort)Globals.Rand.Next(0, ushort.MaxValue);
            }

            internal string ProtectString(string s)
            {
                if (Cache.ContainsKey(s))
                    return Cache[s];

                string res = "";
                foreach (char c in s)
                    res += (char)(c ^ (char)Key);

                Cache[s] = res;
                return res;
            }
            /*private static string UnprotectString(string s)
            {
                string res = "";
                foreach (char c in s)
                    res += (char)(c ^ (char)Key);

                return res;
            }*/

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
            MethodBuilder smethB = new MethodBuilder("UnprotectString", types.String, new TypeSig[] { types.String });
            {
                IMethod getChars = module.ImportMethod<string>("get_Chars");
                IMethod charToString = module.ImportMethod<char>("ToString", new Type[0]);
                IMethod concat = module.ImportMethod<string>("Concat", new Type[] { typeof(string), typeof(string) });
                IMethod getLength = module.ImportMethod<string>("get_Length");

                smethB.AddLocal(types.String); // Result
                smethB.AddLocal(types.Int32); // Index
                smethB.AddLocal(types.Char); // CurrentChar

                // Set variables to default
                smethB.AddInst(OpCodes.Ldstr, ""); // 0
                smethB.AddRefLocal(OpCodes.Stloc, 0); // 1
                smethB.AddInst(OpCodes.Ldc_I4, 0); // 2
                smethB.AddRefLocal(OpCodes.Stloc, 1); // 3

                // Check length
                smethB.AddRefInst(OpCodes.Br, "IL_CHECK"); // 4

                // Get current char
                smethB.AddRefArg("IL_CHAR", OpCodes.Ldarg, 0); // 5
                smethB.AddRefLocal(OpCodes.Ldloc, 1); // 6
                smethB.AddInst(OpCodes.Callvirt, getChars); // 7

                // Xor the current char
                smethB.AddInst(OpCodes.Ldc_I4, (int)s.Key); // 8
                smethB.AddInst(OpCodes.Xor); // 9
                smethB.AddInst(OpCodes.Conv_U2); // 10
                smethB.AddRefLocal(OpCodes.Stloc, 2); // 11

                // Concat current char with result
                smethB.AddRefLocal(OpCodes.Ldloc, 0); // 12 str1
                smethB.AddRefLocal(OpCodes.Ldloca, 2); // 13
                smethB.AddInst(OpCodes.Callvirt, charToString); // 14 str2
                smethB.AddInst(OpCodes.Call, concat); // 15
                smethB.AddRefLocal(OpCodes.Stloc, 0); // 16

                // Increment index
                smethB.AddRefLocal(OpCodes.Ldloc, 1); // 17
                smethB.AddInst(OpCodes.Ldc_I4, 1); // 18
                smethB.AddInst(OpCodes.Add); // 19
                smethB.AddRefLocal(OpCodes.Stloc, 1); // 20

                // Check index with length
                smethB.AddRefLocal("IL_CHECK", OpCodes.Ldloc, 1); // 21
                smethB.AddRefArg(OpCodes.Ldarg, 0); // 22
                smethB.AddInst(OpCodes.Callvirt, getLength); // 23
                smethB.AddRefInst(OpCodes.Blt, "IL_CHAR"); // 24

                // Return result
                smethB.AddRefLocal(OpCodes.Ldloc, 0); // 25
                smethB.AddInst(OpCodes.Ret); // 26
            }

            MethodDef smeth = smethB.Get();
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
