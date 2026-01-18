using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

using TroiletCore;
using TroiletProt_DotNet.Extensions;

namespace TroiletProt_DotNet.Protections
{
    internal class Embedder : ProtectionBase
    {
        public class EmbedderSession : ProtectionSession
        {
            internal TypeDef? Type = null;
            internal Dictionary<UTF8String, UTF8String> _ResNames = new();

            internal long PreCompSize = 0;
            internal long PostCompSize = 0;

            public EmbedderSession(ModuleDef module) : base(module) {}

            public string GetResName(UTF8String name)
            {
                if (_ResNames.TryGetValue(name, out var res))
                    return res;

                return _ResNames[name] = name.Protect();
            }

            public override ProtectionStatistics EndSession()
            {
                if (Type != null)
                    Module.Types.Add(Type);

                return new ProtectionStatistics()
                {
                    Message = (Type == null ? "Nothing to embed" : "{0} -> {1}"),
                    Params = new object[]
                    {
                        Utils.ToSize(PreCompSize),
                        Utils.ToSize(PostCompSize)
                    }
                };
            }
        }

        private MethodDef? _getRes;

        public override string Name { get; protected set; } = "Embedder";

        public override bool CanProtect() => PluginConfig.Instance.GetValueBool("Protections", "embed_prot", true);
        public override ProtectionSession StartSession(ModuleDef module)
        {
            EmbedderSession s = new EmbedderSession(module);
            if (!module.HasResources) // Skip this protection, as there is nothing to protect
                return s;

            ICorLibTypes types = module.CorLibTypes;
            s.Type = Globals.CreateType<Embedder>(module);

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    EmbeddedResource[] embeds = module.Resources.OfType<EmbeddedResource>().ToArray();
                    bw.Write(embeds.Length);
                    foreach (var res in embeds)
                    {
                        bw.Write(s.GetResName(res.Name));
                        uint len = res.Length;
                        bw.Write((int)len);
                        s.PreCompSize += len;

                        var r = res.CreateReader();
                        byte[] data = r.ReadBytes((int)len);
                        bw.Write(data, 0, (int)len);

                        module.Resources.Remove(res);
                    }
                }

                using (var o = new MemoryStream())
                {
                    using (var ds = new DeflateStream(o, CompressionLevel.Optimal))
                    {
                        byte[] data = ms.ToArray();
                        ds.Write(data, 0, data.Length);
                    }

                    byte[] comp = o.ToArray();
                    s.PostCompSize = comp.LongLength;
                    module.Resources.Add(new EmbeddedResource("EmbedderData", comp));
                }
            }

            MethodBuilder getResB = new MethodBuilder("GetResource", new SZArraySig(types.Byte), new TypeSig[] { types.String, types.UInt64 });
            // GetResource
            {
                getResB.AddInst(OpCodes.Ldnull);
                getResB.AddInst(OpCodes.Ret);
            }

            s.Type.Methods.Add(_getRes = getResB.Get());

            return s;
        }

        public override void OnType(ProtectionSession session, TypeDef type)
        {
            EmbedderSession s = (EmbedderSession)session;

            //TODO: Add logic for finding and replacing the wrapper GetResource
        }
    }
}
