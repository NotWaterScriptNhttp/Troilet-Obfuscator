using System;
using System.Runtime.CompilerServices;

using dnlib.DotNet;

using TroiletProt_DotNet.Enums;

namespace TroiletProt_DotNet.Protections
{
    public abstract class ProtectionBase
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckLevel(IHasCustomAttribute attr, ProtectionLevel level, ProtectionLevel val = ProtectionLevel.Full) => (Globals.GetLevel(attr, val) & level) == level;

        public abstract string Name { get; protected set; }

        public abstract bool CanProtect();
        public abstract ProtectionSession StartSession(ModuleDef module);

        public virtual void OnGlobalType(ProtectionSession session, TypeDef gtype) {}
        public virtual void OnType(ProtectionSession session, TypeDef type) {}
    }
}
