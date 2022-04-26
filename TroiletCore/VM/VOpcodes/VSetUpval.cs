namespace TroiletCore.VM.VOpcodes
{
    internal class VSetUpval : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "UpValues[Instruction[REG_B]] = Stack[Instruction[REG_A]]";
    }
}
