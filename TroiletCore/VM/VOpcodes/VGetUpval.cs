namespace TroiletCore.VM.VOpcodes
{
    internal class VGetUpval : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = UpValues[Instruction[REG_B]]";
    }
}
