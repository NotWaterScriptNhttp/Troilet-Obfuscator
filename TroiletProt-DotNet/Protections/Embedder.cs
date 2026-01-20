using System;
using System.IO;
using System.Reflection;
using System.IO.Compression;
using System.Collections.Generic;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

using TroiletCore;
using TroiletProt_DotNet.Attributes;
using TroiletProt_DotNet.Extensions;

namespace TroiletProt_DotNet.Protections
{
    internal class Embedder : ProtectionBase
    {
        public class EmbedderSession : ProtectionSession
        {
            internal TypeDef? Type = null;
            internal List<MethodDef> Readers = new();

            internal long PreCompSize = 0;
            internal long PostCompSize = 0;

            public EmbedderSession(ModuleDef module) : base(module) {}

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
        public override ProtectionSession StartSession(ModuleDef mdl)
        {
            EmbedderSession s = new EmbedderSession(mdl);
            if (!mdl.HasResources) // Skip this protection, as there is nothing to protect
                return s;

            List<EmbeddedResource> embeds = new();
            foreach (var res in mdl.Resources.OfType<EmbeddedResource>())
            {
                // Embedding baml files isn't supported currently
                if (res.Name.EndsWith(".g.resources"))
                    continue;

                embeds.Add(res);
            }
            
            // Compressing one file doesn't really have a benefit, so we skip it
            if (embeds.Count <= 1)
                return s;

            ICorLibTypes types = mdl.CorLibTypes;
            TypeSig barr = new SZArraySig(types.Byte);

            foreach (var t in mdl.Types)
            {
                foreach (var m in t.Methods)
                    if (m.HasCustomAttributes && m.IsStatic && m.ReturnType.IsSame(barr) && m.Parameters.Count == 1 && m.Parameters[0].Type == types.String)
                    {
                        foreach (var ca in m.CustomAttributes)
                            if (EmbedReaderAttribute.CheckAttribute(ca))
                            {
                                s.Readers.Add(m);
                                m.CustomAttributes.Remove(ca);
                                break;
                            }
                    }
            }

            s.Type = Globals.CreateType<Embedder>(mdl);

            byte[] cenameBytes = new byte[Globals.Rand.Next(8, 32)];
            Globals.Rand.NextBytes(cenameBytes);
            string cename = cenameBytes.ToWide();

            // Create compressed resource
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(embeds.Count);
                    foreach (var res in embeds)
                    {
                        bw.Write(res.Name);
                        uint len = res.Length;
                        bw.Write((int)len);
                        s.PreCompSize += len;

                        var r = res.CreateReader();
                        byte[] data = r.ReadBytes((int)len);
                        bw.Write(data, 0, (int)len);

                        mdl.Resources.Remove(res);
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
                    mdl.Resources.Add(new EmbeddedResource(cename, comp));
                }
            }

            Type resType = typeof(Dictionary<string, byte[]>);
            TypeSig resSig = mdl.ImportAsTypeSig(resType);
            FieldDef resFld = s.Type.AddField("_Resources", resSig);

            MethodBuilder decompressB = new MethodBuilder("Decompress", types.Void);
            MethodBuilder getResB = new MethodBuilder("GetResource", new SZArraySig(types.Byte), new TypeSig[] { types.String });
            // Decompress
            {
                IMethod resCtor = mdl.ImportCtor(resType, new Type[0]);
                IMethod getAssembly = mdl.ImportMethod<Assembly>("GetExecutingAssembly", new Type[0]);
                IMethod getRStream = mdl.ImportMethod<Assembly>("GetManifestResourceStream", new Type[] { typeof(string) });
                IMethod exCtor = mdl.ImportCtor<ApplicationException>(new Type[] { typeof(string) });
                IMethod defCtor = mdl.ImportCtor<DeflateStream>(new Type[] { typeof(Stream), typeof(CompressionMode) });
                IMethod brCtor = mdl.ImportCtor<BinaryReader>(new Type[] { typeof(Stream) });
                IMethod brReadInt32 = mdl.ImportMethod<BinaryReader>("ReadInt32");
                IMethod brReadString = mdl.ImportMethod<BinaryReader>("ReadString");
                IMethod brReadBytes = mdl.ImportMethod<BinaryReader>("ReadBytes");
                IMethod addRes = mdl.ImportMethod(resType, "set_Item");
                IMethod dispose = mdl.ImportMethod<IDisposable>("Dispose");

                decompressB.AddLocal(types.Object); // Reader
                decompressB.AddLocal(types.Int32); // RCount
                decompressB.AddLocal(types.Int32); // Index
                decompressB.AddLocal(types.Object); // DelfateStream
                decompressB.AddLocal(types.Object); // MainfestResource

                decompressB.AddInst(OpCodes.Ldc_I4, 0);
                decompressB.AddRefLocal(OpCodes.Stloc, 2);

                // Create resource dictionary
                decompressB.AddInst(OpCodes.Newobj, resCtor);
                decompressB.AddInst(OpCodes.Stsfld, resFld);

                // Get stream for compressed resource
                decompressB.AddInst(OpCodes.Call, getAssembly);
                decompressB.AddInst(OpCodes.Ldstr, cename);
                decompressB.AddInst(OpCodes.Callvirt, getRStream);
                decompressB.AddRefLocal(OpCodes.Stloc, 4);

                // Check if the resource was found
                decompressB.AddRefLocal(OpCodes.Ldloc, 4);
                decompressB.AddInst(OpCodes.Ldnull);
                decompressB.AddInst(OpCodes.Cgt_Un);
                decompressB.AddRefInst(OpCodes.Brtrue, "IL_DECOMPRESS");

                // Throw exception
                decompressB.AddInst(OpCodes.Ldstr, "Failed to get compressed resource!");
                decompressB.AddInst(OpCodes.Newobj, exCtor);
                decompressB.AddInst(OpCodes.Throw);

                // Create decompression stream
                decompressB.AddRefLocal("IL_DECOMPRESS", OpCodes.Ldloc, 4);
                decompressB.AddInst(OpCodes.Ldc_I4, 0);
                decompressB.AddInst(OpCodes.Newobj, defCtor);
                decompressB.AddInst(OpCodes.Dup);
                decompressB.AddRefLocal(OpCodes.Stloc, 3);

                // Create BinaryReader
                decompressB.AddInst(OpCodes.Newobj, brCtor);
                decompressB.AddRefLocal(OpCodes.Stloc, 0);

                // Read number of resources
                decompressB.AddRefLocal(OpCodes.Ldloc, 0);
                decompressB.AddInst(OpCodes.Callvirt, brReadInt32);
                decompressB.AddRefLocal(OpCodes.Stloc, 1);
                decompressB.AddRefInst(OpCodes.Br, "IL_CHECKLOOP");

                // Read resource data
                decompressB.AddInst("IL_RESREAD", OpCodes.Ldsfld, resFld); // Load resource dictionary
                decompressB.AddRefLocal(OpCodes.Ldloc, 0);
                decompressB.AddInst(OpCodes.Callvirt, brReadString); // Resource name
                decompressB.AddRefLocal(OpCodes.Ldloc, 0);
                decompressB.AddInst(OpCodes.Dup); // We will use BinaryReader twice in a row
                decompressB.AddInst(OpCodes.Callvirt, brReadInt32); // Resource data len
                decompressB.AddInst(OpCodes.Callvirt, brReadBytes); // Read resource data
                decompressB.AddInst(OpCodes.Callvirt, addRes); // Adds the resource to the dictionary

                // Increment index
                decompressB.AddRefLocal(OpCodes.Ldloc, 2);
                decompressB.AddInst(OpCodes.Ldc_I4, 1);
                decompressB.AddInst(OpCodes.Add);
                decompressB.AddRefLocal(OpCodes.Stloc, 2);

                // Check loop
                decompressB.AddRefLocal("IL_CHECKLOOP", OpCodes.Ldloc, 2);
                decompressB.AddRefLocal(OpCodes.Ldloc, 1);
                decompressB.AddRefInst(OpCodes.Blt, "IL_RESREAD");

                decompressB.AddRefLocal(OpCodes.Ldloc, 4); // ManifestStream
                decompressB.AddRefLocal(OpCodes.Ldloc, 3); // DeflateStream
                decompressB.AddRefLocal(OpCodes.Ldloc, 0); // BinaryReader
                decompressB.AddInst(OpCodes.Callvirt, dispose); // BinaryReader.Dispose
                decompressB.AddInst(OpCodes.Callvirt, dispose); // DeflateStream.Dispose
                decompressB.AddInst(OpCodes.Callvirt, dispose); // ManifestStream.Dispose

                decompressB.AddInst(OpCodes.Ret);
            }
            // GetResource
            {
                IMethod tryGet = mdl.ImportMethod(resType, "TryGetValue");

                getResB.AddLocal(new SZArraySig(types.Byte));

                // Check if the resource was loaded
                getResB.AddInst(OpCodes.Ldsfld, resFld);
                getResB.AddRefArg(OpCodes.Ldarg, 0);
                getResB.AddRefLocal(OpCodes.Ldloca, 0);
                getResB.AddInst(OpCodes.Callvirt, tryGet);
                getResB.AddRefInst(OpCodes.Brtrue, "IL_RET");

                // Set default value (null)
                getResB.AddInst(OpCodes.Ldnull);
                getResB.AddRefLocal(OpCodes.Stloc, 0);

                // Return value
                getResB.AddRefLocal("IL_RET", OpCodes.Ldloc, 0);
                getResB.AddInst(OpCodes.Ret);
            }

            MethodDef meth = decompressB.Get();
            s.Type.Methods.Add(meth);
            s.Type.Methods.Add(_getRes = getResB.Get());

            Globals.CCtor.AddInst(OpCodes.Call, meth);

            return s;
        }

        public override void OnType(ProtectionSession session, TypeDef type)
        {
            EmbedderSession s = (EmbedderSession)session;

            foreach (var r in s.Readers)
            {
                CilBody b = r.Body;
                b.Instructions.Clear();

                b.Instructions.Add(new Instruction(OpCodes.Ldarg_0));
                b.Instructions.Add(new Instruction(OpCodes.Call, _getRes));
                b.Instructions.Add(new Instruction(OpCodes.Ret));
            }
        }
    }
}
