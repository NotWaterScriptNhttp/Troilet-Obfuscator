using dnlib.DotNet;

namespace TroiletProt_DotNet.Protections
{
    public interface IProtectionSession
    {
        public ModuleDef Module { get; internal set; }

        void EndSession();
    }
}
