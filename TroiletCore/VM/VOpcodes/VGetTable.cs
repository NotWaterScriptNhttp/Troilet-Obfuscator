namespace TroiletCore.VM.VOpcodes
{
    internal class VGetTable : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = Stack[Instruction[REG_B]][(Constants[Instruction[REG_C] + 1] == nil and Stack[Instruction[REG_C]] or Constants[Instruction[REG_C] + 1])]";
    }
}
