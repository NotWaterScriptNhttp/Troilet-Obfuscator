namespace TroiletCore.VM.VOpcodes
{
    internal class VReturn : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => @"local len
if Instruction[REG_B] == 0 then
    len = Top - Instruction[REG_A] + 1
else
    len = Instruction[REG_B] - 1
end

return unpack(Stack, Instruction[REG_A], Instruction[REG_A] + len - 1)";
    }
}
