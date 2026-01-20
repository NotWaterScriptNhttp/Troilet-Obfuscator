using System;

using dnlib.DotNet;

namespace TroiletProt_DotNet.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EmbedReaderAttribute : Attribute
    {
        public static bool CheckAttribute(CustomAttribute ca) => ca.TypeFullName == typeof(EmbedReaderAttribute).FullName;
    }
}
