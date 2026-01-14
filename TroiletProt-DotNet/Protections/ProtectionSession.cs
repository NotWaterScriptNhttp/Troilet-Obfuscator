using dnlib.DotNet;

namespace TroiletProt_DotNet.Protections
{
    public class ProtectionStatistics
    {
        public string Message = string.Empty;
        public object[]? Params = null;

        public override string ToString() => Params != null ? string.Format(Message, Params) : Message;
    }

    public abstract class ProtectionSession
    {
        public ModuleDef Module { get; protected set; }  

        public ProtectionSession(ModuleDef module) => Module = module;

        public abstract ProtectionStatistics EndSession();
    }
}
