namespace TroiletCore.VM.VOpcodes
{
    internal class VGetTable : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = Stack[Instruction[REG_B]][(Instruction[REG_C] > 255 and Constants[Instruction[REG_C] - 255] or Stack[Instruction[REG_C]])]";
    }
}
