using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TroiletProt_DotNet.Protections
{
    internal class ConstantProtection : ProtectionBase
    {
        public class ConstantSession : ProtectionSession
        {
            public ConstantSession(ModuleDef mdl) => Module = mdl;

            internal TypeDef? Type = null;
            internal uint Val = 0;

            public override ProtectionStatistics EndSession()
            {
                if (Val > 0)
                    Module.Types.Add(Type);

                return new ProtectionStatistics()
                {
                    Message = "Protected {0} constants",
                    Params = new object[] { Val }
                };
            }
        }

        public override ProtectionSession StartSession(ModuleDef module)
        {
            ConstantSession s = new ConstantSession(module);
            s.Type = new TypeDefUser("ConstantProtectionType");

            ICorLibTypes types = module.CorLibTypes;
            MethodDef smeth = new MethodDefUser("UnprotectString", new MethodSig(CallingConvention.Default, 1, types.String, new SZArraySig(types.Byte)));
            s.Type.Methods.Add(smeth);

            void CheckType(TypeDef type, int depth = 0)
            {
                if (depth >= 10)
                    return;

                foreach (TypeDef t in type.NestedTypes)
                    CheckType(t, depth + 1);

                
            }

            foreach (TypeDef t in module.Types)
                CheckType(t);

            return s;
        }
    }
}
