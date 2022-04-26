namespace TroiletCore.VM.VOpcodes
{
    internal class VTailcall : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => @"local p
if Instruction[REG_B] == 0 then
    p = Top - Instruction[REG_A]
else
    p = Instruction[REG_B] - 1
end

return Stack[Instruction[REG_A]](unpack(Stack, Instruction[REG_A] + 1, Instruction[REG_A] + p))";
    }
}
