namespace TroiletCore.VM.VOpcodes
{
    internal class VMove : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]]=Stack[Instruction[REG_B]]";
    }
}
