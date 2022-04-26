namespace TroiletCore.VM.VOpcodes
{
    internal class VTestSet : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => @"if (not Stack[Instruction[REG_B]]) ~= (Instruction[REG_C] ~= 0) then
    Stack[Instruction[REG_A]] = Stack[Instruction[REG_B]]
    PC = PC + Instructions[PC][REG_B]
end
PC = PC + 1";
    }
}
