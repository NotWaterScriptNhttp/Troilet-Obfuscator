using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace TroiletProt_DotNet
{
    internal class MethodBuilder
    {
        private enum RefType : byte
        {
            Instruction,
            Local
        }
        private struct IdxRef
        {
            public RefType Type;
            public int Index;

            public IdxRef(RefType type, int idx)
            {
                Type = type;
                Index = idx;
            }
        }

        private MethodDef _Meth;
        private bool _Changed = false;
        private Dictionary<string, Instruction> _NamedInstrs = new Dictionary<string, Instruction>();

        // Resolves indexes to valid references
        private void Resolve()
        {
            int lCnt = _Meth.Body.Variables.Count;
            int iCnt = _Meth.Body.Instructions.Count;
            foreach (Instruction i in _Meth.Body.Instructions)
            {
                if (i.Operand is not IdxRef)
                    continue;

                IdxRef ir = (IdxRef)i.Operand;
                switch (ir.Type)
                {
                    case RefType.Local:
                        if (ir.Index >= lCnt)
                            throw new IndexOutOfRangeException();

                        i.Operand = _Meth.Body.Variables[ir.Index];
                        break;

                    case RefType.Instruction:
                        if (ir.Index >= iCnt)
                            throw new IndexOutOfRangeException();

                        i.Operand = _Meth.Body.Instructions[ir.Index];
                        break;
                }
            }
        }

        public MethodBuilder(MethodDef m)
        {
            if (!m.HasBody)
                m.Body = new CilBody();

            _Meth = m;
        }
        public MethodBuilder(string name, TypeSig ret, TypeSig[]? args = null, MethodAttributes attrs = MethodAttributes.Static | MethodAttributes.Public)
        {
            MethodSig sig;
            if (args != null)
                sig = new MethodSig(CallingConvention.Default, (uint)args.Length, ret, args);
            else sig = new MethodSig(CallingConvention.Default, 0, ret);

            _Meth = new MethodDefUser(name, sig, attrs);
            _Meth.Body = new CilBody();
        }

        public ushort AddLocal(TypeSig sig)
        {
            int idx = _Meth.Body.Variables.Count;
            _Meth.Body.Variables.Add(new Local(sig));
            return (ushort)idx;
        }

        public MethodDef Get()
        {
            if (_Changed)
            {
                if (_Meth.HasBody)
                {
                    Resolve();
                    _Meth.Body.OptimizeBranches();
                    _Meth.Body.OptimizeMacros();
                }

                _Changed = false;
            }

            return _Meth;
        }
    }
}
