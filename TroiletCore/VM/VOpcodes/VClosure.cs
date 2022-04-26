namespace TroiletCore.VM.VOpcodes
{
    internal class VClosure : VOpcode // needs to be tested
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => $@"local func = Protos[Instruction[REG_B]]
local nups = #func.Debug.UpValues
local upvals

if nups ~= 0 then
    upvals = {"{}"}

    for i=1,nups do
        local ps = Instructions[PC + i - 1]
        if ps[I_OPCODE] == {VOpcodeGenerator.vOpcodes[LuaLib.Emit.OpCodes.MOVE].opcode} then
            upvals[i - 1] = ps[REG_B]
        elseif ps[I_OPCODE] == {VOpcodeGenerator.vOpcodes[LuaLib.Emit.OpCodes.GETUPVAL].opcode} then
            upvals[i - 1] = UpValues[ps[REG_B]]
        end
    end
    PC = PC + 1
end

Wrap(func, Globals, upvals)";
    }
}
