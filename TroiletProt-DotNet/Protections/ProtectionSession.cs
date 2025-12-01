using dnlib.DotNet;

namespace TroiletProt_DotNet.Protections
{
    public abstract class ProtectionSession
    {
        public ModuleDef Module { get; protected set; }  

        public abstract void EndSession();
    }
}
