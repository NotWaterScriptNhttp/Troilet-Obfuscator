namespace TroiletCore.VM.VOpcodes
{
    internal class VSetTable : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]][(Constants[Instruction[REG_B] + 1] == nil and Stack[Instruction[REG_B]] or Constants[Instruction[REG_B] + 1])] = (Constants[Instruction[REG_C] + 1] == nil and Stack[Instruction[REG_C]] or Constants[Instruction[REG_C] + 1])";
    }
}
