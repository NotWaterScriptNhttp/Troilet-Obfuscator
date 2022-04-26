using System;
using System.Collections.Generic;

using TroiletCore.RandomStuff;

using LuaLib.Emit;

namespace TroiletCore.VM
{
    internal static class VOpcodeGenerator
    {
        public static Dictionary<OpCodes, VOpcode> vOpcodes { get; private set; }
        public static List<OpCodes> UsedOpcodes = new List<OpCodes>();

        static VOpcodeGenerator()
        {
            int rol(int x, int n) => (x << n) | (x >> (16 - n));

            OpCodes[] opcodes = new OpCodes[]
            {
                OpCodes.MOVE,

                OpCodes.LOADK,
                OpCodes.LOADBOOL,
                OpCodes.LOADNIL,

                OpCodes.GETUPVAL,
                OpCodes.GETGLOBAL,
                OpCodes.GETTABLE,

                OpCodes.SETGLOBAL,
                OpCodes.SETUPVAL,
                OpCodes.SETTABLE,

                OpCodes.NEWTABLE,

                OpCodes.SELF,

                OpCodes.ADD,
                OpCodes.SUB,
                OpCodes.MUL,
                OpCodes.DIV,
                OpCodes.MOD,
                OpCodes.POW,
                OpCodes.UNM,
                OpCodes.NOT,
                OpCodes.LEN,

                OpCodes.CONCAT,

                OpCodes.JMP,

                OpCodes.EQ,
                OpCodes.LT,
                OpCodes.LE,

                OpCodes.TEST,
                OpCodes.TESTSET,

                OpCodes.CALL,
                OpCodes.TAILCALL,
                OpCodes.RETURN,

                OpCodes.FORLOOP,
                OpCodes.FORPREP,
                OpCodes.TFORLOOP,

                OpCodes.SETLIST,

                OpCodes.CLOSE,
                OpCodes.CLOSURE,

                OpCodes.VARARG
            };

            vOpcodes = new Dictionary<OpCodes, VOpcode>();

            opcodes.Shuffle();

            for (int i = 0; i < opcodes.Length; i++)
            {
                OpCodes op = opcodes[i];

                VOpcode vop = new VOpcode(op);

                vop.opcode = (ushort)(rol(i, i % Helpers.random.Next(1, 19)) ^ Helpers.random.Next(1, ushort.MaxValue / Math.Max(i, 1))); // Do magic

                //TODO: check for opcode collision

                vOpcodes.Add(op, vop);
            }
        }
    }
}
