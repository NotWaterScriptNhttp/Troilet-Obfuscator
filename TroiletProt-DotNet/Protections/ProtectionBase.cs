using System;

using dnlib.DotNet;

namespace TroiletProt_DotNet.Protections
{
    public abstract class ProtectionBase
    {
        public virtual void OnModule(ModuleDef mdl) {}

        public virtual void OnMethod(MethodDef method) {}
        public virtual void OnField(FieldDef field) {}
        public virtual void OnProperty(PropertyDef property) {}
        public virtual void OnEvent(EventDef eventDef) {}

        // This function is ran last
        public virtual void OnType(TypeDef type) {}
    }
}
