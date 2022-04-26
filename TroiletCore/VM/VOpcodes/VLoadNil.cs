namespace TroiletCore.VM.VOpcodes
{
    internal class VLoadNil : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "for i=Instruction[REG_A],Instruction[REG_B]do\n     Stack[i]=nil\nend";
    }
}
