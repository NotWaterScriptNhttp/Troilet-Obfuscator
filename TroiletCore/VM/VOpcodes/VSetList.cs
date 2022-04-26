namespace TroiletCore.VM.VOpcodes
{
    internal class VSetList : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => @"local len = Instruction[REG_B]
local offset

if len == 0 then
    len = Top - Instruction[REG_A]
end
local c = Instruction[REG_C]
if c == 0 then
    c = Instruction['raw']
    PC = PC + 1
end

offset = (c - 1) * 50

for i=Instruction[REG_A] + 1, Instruction[REG_A] + len do
    table.insert(Stack[Instruction[REG_A]], Stack[i + offset + 1])
end";
    }
}
