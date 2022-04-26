namespace TroiletCore.VM.VOpcodes
{
    internal class VGetGlobal : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = Globals[Constants[Instruction[REG_B] + 1]]";
    }
}
