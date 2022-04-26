namespace TroiletCore.VM.VOpcodes
{
    internal class VTest : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => @"if (not Stack[Instruction[REG_A]]) ~= (Instruction[REG_C] ~= 0) then
    PC = PC + Instructions[PC][REG_B]
end
PC = PC + 1";
    }
}
