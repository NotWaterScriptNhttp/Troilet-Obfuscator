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
            static object _lock = new object();
            static Dictionary<string, string> _SCache = new Dictionary<string, string>();

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
            private static string UnprotectString(string s)
            {
                lock(_lock)
                {
                    if (_SCache.TryGetValue(s, out var res))
                        return res;

                    _SCache[s] = "";
                }

                return s;
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
            ICorLibTypes types = module.CorLibTypes;
            ConstantSession s = new ConstantSession(module);
            s.Type = Globals.CreateType<ConstantProtection>(module);

            Type cacheType = typeof(Dictionary<string, string>);
            TypeSig cacheSig = module.ImportAsSig<Dictionary<string, string>>();
            FieldDef lockFld = s.Type.AddField("_lock", types.Object);
            FieldDef cacheFld = s.Type.AddField("_cache", cacheSig);

            MethodBuilder cctorB = new MethodBuilder(".cctor", types.Void, attrs: MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig);
            MethodBuilder unprotectStrB = new MethodBuilder("UnprotectString", types.String, new TypeSig[] { types.String });
            // .cctor
            {
                IMethod objCtor = module.ImportCtor<object>(new Type[0]);
                IMethod cacheCtor = module.ImportCtor(cacheType, new Type[0]);

                cctorB.AddInst(OpCodes.Newobj, objCtor);
                cctorB.AddInst(OpCodes.Stsfld, lockFld);
                cctorB.AddInst(OpCodes.Newobj, cacheCtor);
                cctorB.AddInst(OpCodes.Stsfld, cacheFld);
                cctorB.AddInst(OpCodes.Ret);
            }
            // UnprotectString
            {
                IMethod monEnter = module.ImportMethod(typeof(Monitor), "Enter", new Type[] { typeof(object), typeof(bool).MakeByRefType() });
                IMethod monExit = module.ImportMethod(typeof(Monitor), "Exit", new Type[] { typeof(object) });

                IMethod cacheTryGet = module.ImportMethod(cacheType, "TryGetValue");
                IMethod cacheSet = module.ImportMethod(cacheType, "set_Item");

                IMethod getChars = module.ImportMethod<string>("get_Chars");
                IMethod charToString = module.ImportMethod<char>("ToString", new Type[0]);
                IMethod concat = module.ImportMethod<string>("Concat", new Type[] { typeof(string), typeof(string) });
                IMethod getLength = module.ImportMethod<string>("get_Length");

                unprotectStrB.AddLocal(types.String); // Result
                unprotectStrB.AddLocal(types.Int32); // Index
                unprotectStrB.AddLocal(types.Char); // CurrentChar
                unprotectStrB.AddLocal(types.Boolean); // Temp

                unprotectStrB.AddEH(
                    "IL_EH1TS", "IL_EH1CS",
                    "IL_EH1CS", "IL_EXITCACHE",
                    ExceptionHandlerType.Finally
                );
                unprotectStrB.AddEH(
                    "IL_EH2TS", "IL_EH2CS",
                    "IL_EH2CS", "IL_EXIT",
                    ExceptionHandlerType.Finally
                );

                // Enter cache monitor
                unprotectStrB.AddInst(OpCodes.Ldc_I4, 0);
                unprotectStrB.AddRefLocal(OpCodes.Stloc, 3);
                unprotectStrB.AddInst("IL_EH1TS", OpCodes.Ldsfld, lockFld);
                unprotectStrB.AddRefLocal(OpCodes.Ldloca, 3);
                unprotectStrB.AddInst(OpCodes.Call, monEnter);

                // Check cache, if found exit
                unprotectStrB.AddInst(OpCodes.Ldsfld, cacheFld);
                unprotectStrB.AddRefArg(OpCodes.Ldarg, 0);
                unprotectStrB.AddRefLocal(OpCodes.Ldloca, 0);
                unprotectStrB.AddInst(OpCodes.Callvirt, cacheTryGet);
                unprotectStrB.AddRefInst(OpCodes.Brtrue, "IL_CACHERET");

                // Leave logic
                unprotectStrB.AddRefInst(OpCodes.Leave, "IL_EXITCACHE");
                unprotectStrB.AddRefInst("IL_CACHERET", OpCodes.Leave, "IL_EXIT");

                // Exit cache monitor (finally statement)
                unprotectStrB.AddRefLocal("IL_EH1CS", OpCodes.Ldloc, 3);
                unprotectStrB.AddRefInst(OpCodes.Brfalse, "IL_ENDFINALLY1");
                unprotectStrB.AddInst(OpCodes.Ldsfld, lockFld);
                unprotectStrB.AddInst(OpCodes.Call, monExit);
                unprotectStrB.AddInst("IL_ENDFINALLY1", OpCodes.Endfinally);

                // Set variables to default
                unprotectStrB.AddInst("IL_EXITCACHE", OpCodes.Ldstr, ""); // 0
                unprotectStrB.AddRefLocal(OpCodes.Stloc, 0); // 1
                unprotectStrB.AddInst(OpCodes.Ldc_I4, 0); // 2
                unprotectStrB.AddRefLocal(OpCodes.Stloc, 1); // 3

                // Check length
                unprotectStrB.AddRefInst(OpCodes.Br, "IL_CHECK"); // 4

                // Get current char
                unprotectStrB.AddRefArg("IL_CHAR", OpCodes.Ldarg, 0); // 5
                unprotectStrB.AddRefLocal(OpCodes.Ldloc, 1); // 6
                unprotectStrB.AddInst(OpCodes.Callvirt, getChars); // 7

                // Xor the current char
                unprotectStrB.AddInst(OpCodes.Ldc_I4, (int)s.Key); // 8
                unprotectStrB.AddInst(OpCodes.Xor); // 9
                unprotectStrB.AddInst(OpCodes.Conv_U2); // 10
                unprotectStrB.AddRefLocal(OpCodes.Stloc, 2); // 11

                // Concat current char with result
                unprotectStrB.AddRefLocal(OpCodes.Ldloc, 0); // 12 str1
                unprotectStrB.AddRefLocal(OpCodes.Ldloca, 2); // 13
                unprotectStrB.AddInst(OpCodes.Callvirt, charToString); // 14 str2
                unprotectStrB.AddInst(OpCodes.Call, concat); // 15
                unprotectStrB.AddRefLocal(OpCodes.Stloc, 0); // 16

                // Increment index
                unprotectStrB.AddRefLocal(OpCodes.Ldloc, 1); // 17
                unprotectStrB.AddInst(OpCodes.Ldc_I4, 1); // 18
                unprotectStrB.AddInst(OpCodes.Add); // 19
                unprotectStrB.AddRefLocal(OpCodes.Stloc, 1); // 20

                // Check index with length
                unprotectStrB.AddRefLocal("IL_CHECK", OpCodes.Ldloc, 1); // 21
                unprotectStrB.AddRefArg(OpCodes.Ldarg, 0); // 22
                unprotectStrB.AddInst(OpCodes.Callvirt, getLength); // 23
                unprotectStrB.AddRefInst(OpCodes.Blt, "IL_CHAR"); // 24

                // Enter cache monitor
                unprotectStrB.AddInst(OpCodes.Ldc_I4, 0);
                unprotectStrB.AddRefLocal(OpCodes.Stloc, 3);
                unprotectStrB.AddInst("IL_EH2TS", OpCodes.Ldsfld, lockFld);
                unprotectStrB.AddRefLocal(OpCodes.Ldloca, 3);
                unprotectStrB.AddInst(OpCodes.Call, monEnter);

                // Add cache value
                unprotectStrB.AddInst(OpCodes.Ldsfld, cacheFld);
                unprotectStrB.AddRefArg(OpCodes.Ldarg, 0);
                unprotectStrB.AddRefLocal(OpCodes.Ldloc, 0);
                unprotectStrB.AddInst(OpCodes.Callvirt, cacheSet);
                unprotectStrB.AddRefInst(OpCodes.Leave, "IL_EXIT");

                // Exit cache monitor
                unprotectStrB.AddRefLocal("IL_EH2CS", OpCodes.Ldloc, 3);
                unprotectStrB.AddRefInst(OpCodes.Brfalse, "IL_ENDFINALLY2");
                unprotectStrB.AddInst(OpCodes.Ldsfld, lockFld);
                unprotectStrB.AddInst(OpCodes.Call, monExit);
                unprotectStrB.AddInst("IL_ENDFINALLY2", OpCodes.Endfinally);

                // Return result
                unprotectStrB.AddRefLocal("IL_EXIT", OpCodes.Ldloc, 0); // 25
                unprotectStrB.AddInst(OpCodes.Ret); // 26
            }

            MethodDef unprotectStr = unprotectStrB.Get();
            s.Type.Methods.Add(cctorB.Get());
            s.Type.Methods.Add(unprotectStr);

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
                                    body.Instructions.Insert(++i, new Instruction(OpCodes.Call, unprotectStr));

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
