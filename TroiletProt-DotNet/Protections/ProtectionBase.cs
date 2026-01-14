using System;

using dnlib.DotNet;

namespace TroiletProt_DotNet.Protections
{
    public abstract class ProtectionBase
    {
        protected const int MAX_DEPTH = 10;

        public abstract ProtectionSession StartSession(ModuleDef module);

        public virtual void OnType(ProtectionSession session, TypeDef type) {}
    }
}
