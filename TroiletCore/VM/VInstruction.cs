using TroiletCore.RandomStuff;

using LuaLib.Emit;

namespace TroiletCore.VM
{
    using System;
    using System.IO;

    internal class VInstruction
    {
        public VOpcode opcode;
        public byte A;
        public ushort B, C; // (A -> 8 bits : (255)) (B -> 9 bits : (511)) (C -> 9 bits : (511))
        public int Bx, sBx; // (Bx -> 18 bits : (262,144)) (sBx -> Bx - MAXARG_sBx : (131,072 / -131,072))

        private Instruction orig;

        public VInstruction(Instruction inst)
        {
            A = (byte)inst.A;
            B = (ushort)inst.B;
            C = (ushort)inst.C;
            Bx = inst.Bx;
            sBx = inst.sBx;

            opcode = VOpcodeGenerator.vOpcodes[inst.Opcode];

            orig = inst;
        }

        public void GetInstruction(Troilet_Bytecode.TroiletWriter tw)
        {
            Registers r = Instruction.RegistersMap.GetRegister(LuaLib.LuaVersion.LUA_VERSION_5_1, opcode.Original);
            bool AddRawInstr = false;

            #region OpMode

            byte mode = 0;

            // doing it like this cause its easier to read
            mode |= (byte)(r.A ? 1 : 0);
            mode |= (byte)((r.B ? 1 : 0) << 1);
            mode |= (byte)((r.C ? 1 : 0) << 2);
            mode |= (byte)((r.Bx ? 1 : 0) << 3);
            mode |= (byte)((r.sBx ? 1 : 0) << 4);

            switch (opcode.Original)
            {
                case OpCodes.SETLIST:
                    mode |= (byte)((r.sBx ? 1 : 0) << 5);
                    AddRawInstr = true;
                    break;
            }

            tw.ProtectedWrite(mode);

            #endregion
            #region Registers

            if (r.A)
                tw.ProtectedWrite(A);
            if (r.B)
                tw.ProtectedWrite(B);
            if (r.Bx)
                tw.ProtectedWrite(Bx);
            if (r.sBx)
                tw.ProtectedWrite(sBx + 131072);
            if (r.C)
                tw.ProtectedWrite(C);
            if (AddRawInstr)
                tw.ProtectedWrite(orig.GetRawInstruction());

            tw.ProtectedWrite(opcode.opcode);

            #endregion

#if DEBUG
            File.AppendAllText("Temp/VM.log",
$@"{opcode.Original} -> ({opcode.opcode}): [
    OpMode: {mode} -> ({Convert.ToString(mode, 2)}),
    A: {A},
    B: {B},
    C: {C},
    Bx: {Bx},
    sBx: {sBx}
]" + "\n"
            );
#endif
        }
    }
}
