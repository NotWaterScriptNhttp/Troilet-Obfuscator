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
}

namespace TroiletProt_DotNet.Extensions
{
    internal static class InstrList
    {
        public static void ResolveIndexes(this IList<Instruction> self)
        {
            int c = self.Count;
            foreach (Instruction i in self)
            {
                if (i.Operand is not InstrIdx)
                    continue;

                int idx = ((InstrIdx)i.Operand).Index;
                if (idx >= c)
                    throw new IndexOutOfRangeException();

                i.Operand = self[idx];
            }
        }
    }
}
