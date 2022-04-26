namespace TroiletCore.VM.VOpcodes
{
    internal class VLoadBool : VOpcode
    {
        // Doing a check on C cause it can be changed
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = (Instruction[REG_B] == 1 and true or false)\nPC = PC + (Instruction[REG_C] == 1 and 1 or 0)";
    }
}
