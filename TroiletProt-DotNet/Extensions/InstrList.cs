using System;
using System.Collections.Generic;

using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Emit
{
    internal struct InstrIdx
    {
        public readonly int Index;
        
        public InstrIdx(int idx) => Index = idx;
    }
    internal struct LocalIdx
    {
        public readonly int Index;

        public LocalIdx(int idx) => Index = idx;
    }
}

namespace TroiletProt_DotNet.Extensions
{
    internal static class InstrList
    {
        public static void Add(this IList<Instruction> self, OpCode oc) => self.Add(new Instruction(oc));
        public static void Add(this IList<Instruction> self, OpCode oc, object operand) => self.Add(new Instruction(oc, operand));

        public static void ResolveIndexes(this IList<Instruction> self, LocalList? locals = null)
        {
            int c = self.Count;
            foreach (Instruction i in self)
            {
                object op = i.Operand;
                if (op is InstrIdx)
                {
                    int idx = ((InstrIdx)i.Operand).Index;
                    if (idx >= c)
                        throw new IndexOutOfRangeException();

                    i.Operand = self[idx];
                }
                else if (locals != null && op is LocalIdx)
                {
                    int idx = ((LocalIdx)i.Operand).Index;
                    if (idx >= locals.Count)
                        throw new IndexOutOfRangeException();

                    i.Operand = locals[idx];
                }
            }
        }
    }
}
