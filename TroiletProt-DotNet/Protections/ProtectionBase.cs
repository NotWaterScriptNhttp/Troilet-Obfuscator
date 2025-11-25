using System;

using dnlib.DotNet;

namespace TroiletProt_DotNet.Protections
{
    public abstract class ProtectionBase
    {
        public abstract IProtectionSession StartSession(ModuleDef module);

        public virtual void OnType(IProtectionSession session, TypeDef type) {}
    }
}
