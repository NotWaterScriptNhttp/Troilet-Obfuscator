using System.Collections.Generic;

namespace TroiletCore.VM
{
    using TroiletCore.VM.VOpcodes;

    using LuaLib.Emit;

    internal class VOpcode
    {
        public OpCodes Original;
        public ushort opcode;

        private VOpcode vop = null;

        protected VOpcode() { }
        public VOpcode(OpCodes opcode)
        {
            Original = opcode;

            switch (opcode)
            {
                case OpCodes.MOVE:
                    vop = new VMove();
                    break;
                case OpCodes.LOADK:
                    vop = new VLoadK();
                    break;
                case OpCodes.LOADBOOL:
                    vop = new VLoadBool();
                    break;
                case OpCodes.LOADNIL:
                    vop = new VLoadNil();
                    break;

                case OpCodes.GETUPVAL:
                    vop = new VGetUpval();
                    break;
                case OpCodes.GETGLOBAL:
                    vop = new VGetGlobal();
                    break;
                case OpCodes.GETTABLE:
                    vop = new VGetTable();
                    break;

                case OpCodes.SETGLOBAL:
                    vop = new VSetGlobal();
                    break;
                case OpCodes.SETUPVAL:
                    vop = new VSetUpval();
                    break;
                case OpCodes.SETTABLE:
                    vop = new VSetTable();
                    break;

                case OpCodes.NEWTABLE:
                    vop = new VNewTable();
                    break;

                case OpCodes.SELF:
                    vop = new VSelf();
                    break;

                case OpCodes.ADD:
                    vop = new VAdd();
                    break;
                case OpCodes.SUB:
                    vop = new VSub();
                    break;
                case OpCodes.MUL:
                    vop = new VMul();
                    break;
                case OpCodes.DIV:
                    vop = new VDiv();
                    break;
                case OpCodes.MOD:
                    vop = new VMod();
                    break;
                case OpCodes.POW:
                    vop = new VPow();
                    break;
                case OpCodes.UNM:
                    vop = new VUnm();
                    break;
                case OpCodes.NOT:
                    vop = new VNot();
                    break;
                case OpCodes.LEN:
                    vop = new VLen();
                    break;

                case OpCodes.CONCAT:
                    vop = new VConcat();
                    break;

                case OpCodes.JMP:
                    vop = new VJmp();
                    break;

                case OpCodes.EQ:
                    vop = new VEq();
                    break;
                case OpCodes.LT:
                    vop = new VLt();
                    break;
                case OpCodes.LE:
                    vop = new VLe();
                    break;

                case OpCodes.TEST:
                    vop = new VTest();
                    break;
                case OpCodes.TESTSET:
                    vop = new VTestSet();
                    break;

                case OpCodes.CALL:
                    vop = new VCall();
                    break;
                case OpCodes.TAILCALL:
                    vop = new VTailcall();
                    break;
                case OpCodes.RETURN:
                    vop = new VReturn();
                    break;

                case OpCodes.FORLOOP:
                    vop = new VForLoop();
                    break;
                case OpCodes.FORPREP:
                    vop = new VForPrep();
                    break;
                case OpCodes.TFORLOOP:
                    vop = new VTForLoop();
                    break;

                case OpCodes.SETLIST:
                    vop = new VSetList();
                    break;

                case OpCodes.CLOSE:
                    vop = new VClose();
                    break;
                case OpCodes.CLOSURE:
                    vop = new VClosure();
                    break;

                case OpCodes.VARARG:
                    break;

                default:
                    throw new System.Exception($"Opcode is not added/supported ({opcode})");
            }
        }

        virtual internal string GetOpCode(ObfuscatorSettings settings)
        {
            if (vop == null)
                return "";

            //Inlined by my hand
            return vop.GetOpCode(settings).Replace("REG_A", settings.A_Idx.ToString()).Replace("REG_B", settings.B_Idx.ToString()).Replace("REG_C", settings.C_Idx.ToString()).Replace("I_OPCODE", settings.OP_Idx.ToString());
        }
    }
}
