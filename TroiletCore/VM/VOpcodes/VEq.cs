namespace TroiletCore.VM.VOpcodes
{
    internal class VEq : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "PC = PC + (((Instruction[REG_B] > 255 and Constants[Instruction[REG_B]] or Instruction[REG_B]) == (Instruction[REG_C] > 255 and Constants[Instruction[REG_C]] or Instruction[REG_C])) ~= Instruction[REG_A] and 1 or 0)";
    }
}
