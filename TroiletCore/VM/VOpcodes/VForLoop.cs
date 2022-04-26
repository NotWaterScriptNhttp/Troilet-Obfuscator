namespace TroiletCore.VM.VOpcodes
{
    internal class VForLoop : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => @"
local idx = Stack[Instruction[REG_A]]
local limit = Stack[Instruction[REG_A] + 1]
local step = Stack[Instruction[REG_A] + 2]

if math.abs(limit) <= math.abs(idx) then
    PC = PC + Stack[Instruction[REG_B]]
    Stack[Instruction[REG_A]] = idx + step
end";
    }
    internal class VForPrep : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => @"local init, limit, step
init = assert(tonumber(Stack[Instruction[REG_A]]), 'must be number')
limit = assert(tonumber(Stack[Instruction[REG_A] + 1]), 'must be number')
step = assert(tonumber(Stack[Instruction[REG_A] + 2]), 'must be number')

Stack[Instruction[REG_A]] = init - step
Stack[Instruction[REG_A] + 1] = limit
Stack[Instruction[REG_A] + 2] = step
PC = PC + Instruction[REG_B]";
    }
    internal class VTForLoop : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => @"
local limit = Stack[Instruction[REG_A] + 1]
local step = Stack[Instruction[REG_A] + 2]

local idx = Stack[Instruction[REG_A]] + step

print('For:', idx, limit, step)

if (0 < step) and (idx <= limit) or (limit < idx) then
    PC = PC + Instruction[REG_B]
    Stack[Instruction[REG_A]] = idx
end
";
    }
}
