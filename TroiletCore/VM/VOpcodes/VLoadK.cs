namespace TroiletCore.VM.VOpcodes
{
    internal class VLoadK : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]]=Constants[Instruction[REG_B] + 1]";
    }
}
