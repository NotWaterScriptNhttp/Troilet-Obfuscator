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
        public class ConstantSession : IProtectionSession
        {
            ModuleDef Module { get; set; }

            void EndSession()
            {
                
            }
        }

        public override IProtectionSession StartSession(ModuleDef module)
        {
            ConstantSession s = new ConstantSession();
            

            return s;
        }
    }
}
