namespace TroiletCore.VM.VOpcodes
{
    internal class VCall : VOpcode // should work
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => @"local p
if Instruction[REG_B] == 0 then
    p = Top - Instruction[REG_A]
else
    p = Instruction[REG_B]
end

local results = {Stack[Instruction[REG_A]](unpack(Stack, Instruction[REG_A] + 1, Instruction[REG_A] + p - 1))}
local num = #results

if Instruction[REG_C] == 0 then
    Top = Instruction[REG_A] + num - 1
else
    num = Instruction[REG_C] - 1
end

for i=1,num do
    Stack[Instruction[REG_A] + i] = results[i]
end";
    }
}
