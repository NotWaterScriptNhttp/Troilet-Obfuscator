using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dnlib.DotNet;

namespace TroiletProt_DotNet.Protections
{
    internal class Embedder : ProtectionBase
    {
        public class EmbedderSession : ProtectionSession
        {
            public override ProtectionStatistics EndSession()
            {
                return new ProtectionStatistics();
            }
        }

        public override ProtectionSession StartSession(ModuleDef module)
        {
            EmbedderSession s = new EmbedderSession();

            return s;
        }
    }
}
