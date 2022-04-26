namespace TroiletCore.VM.VOpcodes
{
    internal class VJmp : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "PC = PC + Instruction[REG_B]";
    }
}
