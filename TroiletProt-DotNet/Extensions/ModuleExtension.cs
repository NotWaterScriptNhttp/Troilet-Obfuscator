using System;
using System.Collections.Generic;

using dnlib.DotNet;

namespace TroiletProt_DotNet.Extensions
{
    internal static class ModuleExtension
    {
        public static TypeDef? ExclusionToType(this ModuleDef mdl, Exclusion ex)
        {
            foreach (TypeDef t in mdl.Types)
                if (t.FullName == ex.Type)
                    return t;

            return null;
        }
    }
}
