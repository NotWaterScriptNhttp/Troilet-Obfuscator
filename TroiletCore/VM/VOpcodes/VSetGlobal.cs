namespace TroiletCore.VM.VOpcodes
{
    internal class VSetGlobal : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Globals[Constants[Instruction[REG_B] + 1]] = Stack[Instruction[REG_A]]";
    }
}
