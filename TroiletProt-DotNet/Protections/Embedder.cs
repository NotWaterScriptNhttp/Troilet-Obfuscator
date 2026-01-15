using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace TroiletProt_DotNet.Protections
{
    internal class Embedder : ProtectionBase
    {
        public class EmbedderSession : ProtectionSession
        {
            internal TypeDef? Type = null;

            public EmbedderSession(ModuleDef module) : base(module) {}

            public override ProtectionStatistics EndSession()
            {
                return new ProtectionStatistics();
            }
        }

        public override ProtectionSession StartSession(ModuleDef module)
        {
            EmbedderSession s = new EmbedderSession(module);
            if (!module.HasResources) // Skip this protection, as there is nothing to protect
                return s;

            ICorLibTypes types = module.CorLibTypes;
            s.Type = Globals.CreateType<Embedder>(module);

            MethodBuilder getResB = new MethodBuilder("GetResource", new SZArraySig(types.Byte), new TypeSig[] { types.String, types.UInt64 });
            // GetResource
            {
                getResB.AddInst(OpCodes.Ret);
            }

            MethodDef getRes = getResB.Get();
            s.Type.Methods.Add(getRes);

            //TODO: Add logic for finding and replacing the wrapper GetResource

            return s;
        }
    }
}
