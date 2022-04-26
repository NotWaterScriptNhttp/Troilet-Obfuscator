using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaLib.Emit;

namespace TroiletPlugin.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int opcodes = 38;
            Dictionary<string, int> regs = new Dictionary<string, int>();

            for (int i = 0; i < opcodes; i++)
            {
                Registers r = Instruction.RegistersMap.GetRegister(LuaLib.LuaVersion.LUA_VERSION_5_1, (OpCodes)i);

                string mode = "";

                if (r.A)
                    mode += "A";
                if (r.B)
                    mode += "B";
                if (r.C)
                    mode += "C";
                if (r.Bx)
                    mode += "Bx";
                if (r.sBx)
                    mode += "sBx";

                if (regs.TryGetValue(mode, out int l))
                    regs[mode] += 1;
                else regs[mode] = 1;
            }

            foreach(KeyValuePair<string, int> kvp in regs)
            {
                Console.WriteLine($"Mode: '{kvp.Key}' -> {kvp.Value}");
            }

            ulong test = 0;
            test |= (255UL) << 56;
            test |= (7UL) << 52;
            string s = Convert.ToString((long)test, 2);

            Console.WriteLine($"Bin: '{s}' Len: {s.Length}");

            Console.ReadLine();
        }
    }
}
