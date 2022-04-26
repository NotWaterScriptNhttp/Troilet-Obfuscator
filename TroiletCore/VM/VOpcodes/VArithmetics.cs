namespace TroiletCore.VM.VOpcodes
{
    internal class VAdd : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = (Instruction[REG_B] > 255 and Constants[Instruction[REG_B] - 255] or Stack[Instruction[REG_B]]) + (Instruction[REG_C] > 255 and Constants[Instruction[REG_C] - 255] or Stack[Instruction[REG_C]])";
    }
    internal class VSub : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = (Instruction[REG_B] > 255 and Constants[Instruction[REG_B] - 255] or Stack[Instruction[REG_B]]) - (Instruction[REG_C] > 255 and Constants[Instruction[REG_C] - 255] or Stack[Instruction[REG_C]])";
    }
    internal class VMul : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = (Instruction[REG_B] > 255 and Constants[Instruction[REG_B] - 255] or Stack[Instruction[REG_B]]) * (Instruction[REG_C] > 255 and Constants[Instruction[REG_C] - 255] or Stack[Instruction[REG_C]])";
    }
    internal class VDiv : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = (Instruction[REG_B] > 255 and Constants[Instruction[REG_B] - 255] or Stack[Instruction[REG_B]]) / (Instruction[REG_C] > 255 and Constants[Instruction[REG_C] - 255] or Stack[Instruction[REG_C]])";
    }
    internal class VMod : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = (Instruction[REG_B] > 255 and Constants[Instruction[REG_B] - 255] or Stack[Instruction[REG_B]]) % (Instruction[REG_C] > 255 and Constants[Instruction[REG_C] - 255] or Stack[Instruction[REG_C]])";
    }
    internal class VPow : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = (Constants[Instruction[REG_B] + 1] == nil and Stack[Instruction[REG_B]] or Constants[Instruction[REG_B] + 1]) ^ (Constants[Instruction[REG_C] + 1] == nil and Stack[Instruction[REG_C]] or Constants[Instruction[REG_C] + 1])";
    }
    internal class VUnm : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = -Stack[Instruction[REG_B]]";
    }
    internal class VNot : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = not Stack[Instruction[REG_B]]";
    }
    internal class VLen : VOpcode // Not tested
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = #Stack[Instruction[REG_B]]";
    }
}
