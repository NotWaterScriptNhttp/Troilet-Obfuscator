using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace TroiletProt_DotNet.Protections
{
    internal class Embedder : ProtectionBase
    {
        private struct CompressedEmbedData {
            public string Name;
            public byte[] Data;
        }
        public class EmbedderSession : ProtectionSession
        {
            internal TypeDef? Type = null;
            internal Dictionary<string, string> _ResNames = new();

            public EmbedderSession(ModuleDef module) : base(module) {}

            public string GetResName(string name)
            {
                if (_ResNames.TryGetValue(name, out var res))
                    return res;

                return _ResNames[name] = Globals.GetRandomString();
            }

            public override ProtectionStatistics EndSession()
            {
                Module.Types.Add(Type);
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

            using (var ms = new MemoryStream())
                using (var bw = new BinaryWriter(ms))
            {
                EmbeddedResource[] embeds = module.Resources.OfType<EmbeddedResource>().ToArray();
                foreach (var res in embeds)
                {
                    bw.Write(s.GetResName(res.Name));
                    bw.Write((int)res.Length);

                    var r = res.CreateReader();
                    bw.Write(r.ReadBytes((int)res.Length));

                    module.Resources.Remove(res);
                }

                //TODO: Compress data
                module.Resources.Add(new EmbeddedResource("EmbedderData", ms.ToArray()));
            }

            MethodBuilder getResB = new MethodBuilder("GetResource", new SZArraySig(types.Byte), new TypeSig[] { types.String, types.UInt64 });
            // GetResource
            {
                getResB.AddInst(OpCodes.Ldnull);
                getResB.AddInst(OpCodes.Ret);
            }

            MethodDef getRes = getResB.Get();
            s.Type.Methods.Add(getRes);

            //TODO: Add logic for finding and replacing the wrapper GetResource

            return s;
        }
    }
}
