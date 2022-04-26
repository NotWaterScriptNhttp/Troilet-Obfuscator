namespace TroiletCore.VM.VOpcodes
{
    internal class VNewTable : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "Stack[Instruction[REG_A]] = {}";
    }
}
