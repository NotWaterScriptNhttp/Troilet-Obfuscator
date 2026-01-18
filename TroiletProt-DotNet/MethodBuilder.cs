using System;
using System.Collections.Generic;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

using TroiletProt_DotNet.Attributes;

namespace TroiletProt_DotNet
{
    [ProtectionLevel(Enums.ProtectionLevel.Name)]
    public class MethodBuilder
    {
        [ProtectionLevel(Enums.ProtectionLevel.Full, false)]
        private enum RefType : byte
        {
            Instruction,
            Local,
            Arg
        }
        [ProtectionLevel(Enums.ProtectionLevel.Full, false)]
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
        [ProtectionLevel(Enums.ProtectionLevel.Full, false)]
        private class EHandler
        {
            public string TryStart = string.Empty;
            public string TryEnd = string.Empty;
            public string CatchStart = string.Empty;
            public string CatchEnd = string.Empty;
            public string? FilterStart = null;
            public ExceptionHandlerType Type = ExceptionHandlerType.Catch;
        }

        [ProtectionLevel(Enums.ProtectionLevel.Full, false)]
        private MethodDef _Meth;
        [ProtectionLevel(Enums.ProtectionLevel.Full, false)]
        private bool _Changed = false;
        [ProtectionLevel(Enums.ProtectionLevel.Full, false)]
        private Dictionary<string, Instruction> _NamedInstrs = new Dictionary<string, Instruction>();
        [ProtectionLevel(Enums.ProtectionLevel.Full, false)]
        private List<EHandler> _EHandlers = new List<EHandler>();

        [ProtectionLevel(Enums.ProtectionLevel.Full, false)]
        private Instruction? ResolveInst(string? name, bool @throw = true)
        {
            if (name != null && _NamedInstrs.TryGetValue(name, out Instruction? i))
                return i;

            if (@throw)
                throw new IndexOutOfRangeException("No named instruction!");
            return null;
        }
        [ProtectionLevel(Enums.ProtectionLevel.Full, false)]
        private T CheckNull<T>(T? obj)
        {
            if (obj == null)
                throw new NullReferenceException();

            return obj;
        }

        public MethodBuilder(MethodDef m)
        {
            if (!m.HasBody)
                m.Body = new CilBody();

            _Meth = m;
        }
        public MethodBuilder(string name, TypeSig ret, TypeSig[]? args = null, MethodAttributes attrs = 0)
        {
            MethodSig sig;
            if (args != null)
                sig = new MethodSig(CallingConvention.Default, (uint)args.Length, ret, args);
            else sig = new MethodSig(CallingConvention.Default, 0, ret);

            _Meth = new MethodDefUser(name, sig, MethodAttributes.Static | MethodAttributes.Public | attrs);
            _Meth.Body = new CilBody();
        }

        public Local AddLocal(TypeSig sig)
        {
            Local loc;
            _Meth.Body.Variables.Add(loc = new Local(sig));
            _Changed = true;
            return loc;
        }

        public void AddEH(string tsName, string teName, string csName, string ceName, ExceptionHandlerType type, string? sfName = null)
        {
            _EHandlers.Add(new EHandler()
            {
                TryStart = tsName,
                TryEnd = teName,
                CatchStart = csName,
                CatchEnd = ceName,
                FilterStart = sfName,
                Type = type
            });

            _Changed = true;
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

                            i.Operand = CheckNull(_Meth.Body.Variables[idx]);
                        }
                        break;

                    case RefType.Instruction:
                        {
                            if (ir.Value is string)
                            {
                                i.Operand = CheckNull(ResolveInst((string)ir.Value));
                            }
                            else if (ir.Value is int)
                            {
                                int idx = (int)ir.Value;
                                if (idx >= iCnt)
                                    throw new IndexOutOfRangeException();

                                i.Operand = CheckNull(_Meth.Body.Instructions[idx]);
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

                            i.Operand = CheckNull(_Meth.Parameters[idx]);
                        }
                        break;
                }
            }
            
            _Meth.Body.OptimizeMacros();
            _Meth.Body.OptimizeBranches();

            foreach (EHandler h in _EHandlers)
            {
                ExceptionHandler eh = new ExceptionHandler(h.Type);
                eh.TryStart = ResolveInst(h.TryStart);
                eh.TryEnd = ResolveInst(h.TryEnd);
                eh.HandlerStart = ResolveInst(h.CatchStart);
                eh.HandlerEnd = ResolveInst(h.CatchEnd);
                eh.FilterStart = ResolveInst(h.FilterStart, false);

                _Meth.Body.ExceptionHandlers.Add(eh);
            }
            _EHandlers.Clear();

            _Changed = false;
        }

        public MethodDef Get()
        {
            Resolve();
            return _Meth;
        }
    }
}
