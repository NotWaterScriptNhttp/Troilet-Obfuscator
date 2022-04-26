namespace TroiletCore.VM.VOpcodes
{
    internal class VClose : VOpcode
    {
        internal override string GetOpCode(ObfuscatorSettings settings) => "for i=1,#Stack do\n     if i >= Instruction[REG_A] then\n     Stack[i]=nil\nend\nend";
    }
}
