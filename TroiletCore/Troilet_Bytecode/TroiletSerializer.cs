using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaLib.Emit;

using TroiletCore.VM;
using TroiletCore.RandomStuff;

namespace TroiletCore.Troilet_Bytecode
{
    internal static class TroiletSerializer
    {
        private static TroiletWriter writer;

        private static bool _writeDebug = false;

        private static void SerializeConstants(int count, List<Constant> consts)
        {
            writer.ProtectedWrite(count);
            
            for (int i = 0; i < count; i++)
            {
                Constant con = consts[i];

                //IDEA: give each const an id and shuffle them

                writer.ProtectedWrite((byte)con.Type);

                switch (con.Type)
                {
                    case ConstantType.NIL:
                        writer.ProtectedWrite("NIL");
                        break;
                    case ConstantType.BOOLEAN:
                        writer.ProtectedWrite(con.Value);
                        break;
                    case ConstantType.NUMBER:
                        writer.ProtectedWrite((double)con.Value);
                        break;
                    case ConstantType.STRING:
                        writer.ProtectedWrite((string)con.Value);
                        break;
                    default:
                        throw new Exception($"Can't serialize unknown constant type ({con.Type} -> '{con.Value}')");
                }
            }
        }
        private static void SerializeInstructions(int count, List<Instruction> instrs)
        {
            writer.ProtectedWrite(count);

            for (int i = 0; i < count; i++)
            {
                new VInstruction(instrs[i]).GetInstruction(writer);
                if (!VOpcodeGenerator.UsedOpcodes.Contains(instrs[i].Opcode))
                    VOpcodeGenerator.UsedOpcodes.Add(instrs[i].Opcode);
            }
        }
        private static void SerializeDebug(Function func)
        {

        }

        public static byte[] Serialize(Function func, bool AsBase64 = true, bool writeDebug = false)
        {
            _writeDebug = writeDebug;
            writer = new TroiletWriter();

            writer.ProtectedWrite(writeDebug);

            byte[] steps = new byte[]
            {
                0, // Constants
                1, // Instructions
                2, // Functions
                //3  // Debug (writeDebug == true)
            };

            steps.Shuffle();

            writer.ProtectedWrite(steps.Length);
            writer.ProtectedWrite(func.is_vararg);

            steps.ToList().ForEach(b =>
            {
                writer.ProtectedWrite(b);

                switch (b)
                {
                    case 0:
                        SerializeConstants(func.ConstantCount, func.Constants);
                        break;
                    case 1:
                        SerializeInstructions(func.InstructionCount, func.Instructions);
                        break;
                    case 2:
                        writer.ProtectedWrite(func.FunctionCount);
                        for (int i = 0; i < func.FunctionCount; i++)
                            writer._writer.Write(Serialize(func.Functions[i], false, writeDebug));
                        break;
                    case 3:
                        if (!writeDebug) break;

                        SerializeDebug(func);
                        break;
                }
            });

            return writer.GetBuffer(AsBase64 ? TroiletWriter.BufferFormat.B64 : TroiletWriter.BufferFormat.Raw);
        }
    }
}
