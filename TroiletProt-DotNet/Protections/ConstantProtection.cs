using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroiletProt_DotNet.Protections
{
    internal class ConstantProtection : ProtectionBase
    {
        public class ConstantSession : ProtectionSession
        {
            public ConstantSession(ModuleDef mdl) => Module = mdl;

            public override void EndSession()
            {
                throw new NotImplementedException();
            }
        }

        public override ProtectionSession StartSession(ModuleDef module)
        {
            ConstantSession s = new ConstantSession(module);

            return s;
        }
    }
}
