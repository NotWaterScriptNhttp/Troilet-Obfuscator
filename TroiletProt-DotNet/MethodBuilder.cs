using System;
using System.Collections.Generic;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace TroiletProt_DotNet
{
    internal class MethodBuilder
    {
        private enum RefType : byte
        {
            Instruction,
            Local,
            Arg
        }
        private struct IdxRef
        {
            public RefType Type;
            public object Value;

            public IdxRef(RefType type, int idx)
            {
                Type = type;
                Value = idx;
            }
            public IdxRef(RefType type, string name)
            {
                Type = type;
                Value = name; 
            }
        }

        private MethodDef _Meth;
        private bool _Changed = false;
        private Dictionary<string, Instruction> _NamedInstrs = new Dictionary<string, Instruction>();

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

        public Local AddLocal(TypeSig sig)
        {
            Local loc;
            _Meth.Body.Variables.Add(loc = new Local(sig));
            _Changed = true;
            return loc;
        }

        public void AddInst(OpCode op, object? operand = null)
        {
            _Meth.Body.Instructions.Add(new Instruction(op, operand));
            _Changed = true;
        }
        public void AddInst(string name, OpCode op, object? operand = null)
        {
            var inst = new Instruction(op, operand);
            _NamedInstrs[name] = inst;
            _Meth.Body.Instructions.Add(inst);
            _Changed = true;
        }

        public void AddRefArg(OpCode op, ushort idx) => AddInst(op, new IdxRef(RefType.Arg, idx));
        public void AddRefArg(string name, OpCode op, ushort idx) => AddInst(name, op, new IdxRef(RefType.Arg, idx));

        public void AddRefLocal(OpCode op, ushort idx) => AddInst(op, new IdxRef(RefType.Local, idx));
        public void AddRefLocal(string name, OpCode op, ushort idx) => AddInst(name, op, new IdxRef(RefType.Local, idx));

        public void AddRefInst(OpCode op, int idx) => AddInst(op, new IdxRef(RefType.Instruction, idx));
        public void AddRefInst(OpCode op, string label) => AddInst(op, new IdxRef(RefType.Instruction, label));
        public void AddRefInst(string name, OpCode op, int idx) => AddInst(name, op, new IdxRef(RefType.Instruction, idx));
        public void AddRefInst(string name, OpCode op, string label) => AddInst(name, op, new IdxRef(RefType.Instruction, label));

        // Resolves indexes to valid references
        public void Resolve()
        {
            if (!_Changed || !_Meth.HasBody)
                return;

            int lCnt = _Meth.Body.Variables.Count;
            int iCnt = _Meth.Body.Instructions.Count;
            int aCnt = _Meth.Parameters.Count;
            foreach (Instruction i in _Meth.Body.Instructions)
            {
                if (i.Operand is not IdxRef)
                    continue;

                IdxRef ir = (IdxRef)i.Operand;
                switch (ir.Type)
                {
                    case RefType.Local:
                        {
                            if (ir.Value is not int)
                                throw new ApplicationException("Invalid value type!");

                            int idx = (int)ir.Value;
                            if (idx >= lCnt)
                                throw new IndexOutOfRangeException();

                            i.Operand = _Meth.Body.Variables[idx];
                        }
                        break;

                    case RefType.Instruction:
                        {
                            if (ir.Value is string)
                                if (_NamedInstrs.TryGetValue((string)ir.Value, out var namedInstr))
                                    i.Operand = namedInstr;
                                else throw new ArgumentException("No named instruction!");
                            else if (ir.Value is int)
                            {
                                int idx = (int)ir.Value;
                                if (idx >= iCnt)
                                    throw new IndexOutOfRangeException();

                                i.Operand = _Meth.Body.Instructions[idx];
                            }
                            else throw new ApplicationException("Invalid value type!");
                        }
                        break;

                    case RefType.Arg:
                        {
                            if (ir.Value is not int)
                                throw new ApplicationException("Invalid value type!");

                            int idx = (int)ir.Value;
                            if (idx >= aCnt)
                                throw new IndexOutOfRangeException();

                            i.Operand = _Meth.Parameters[idx];
                        }
                        break;
                }
            }

            _Meth.Body.OptimizeMacros();
            _Meth.Body.OptimizeBranches();
            _Changed = false;
        }

        public MethodDef Get()
        {
            Resolve();
            return _Meth;
        }
    }
}
