namespace TroiletCore.VM.VOpcodes
{
    internal class VConcat : VOpcode // no problem when tested
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "local buffer=Stack[Instruction[REG_B]] for i=Instruction[REG_B]+1,Instruction[REG_C]do\n   buffer = buffer .. Stack[i]\nend\nStack[Instruction[REG_A]] = buffer";
    }
}
