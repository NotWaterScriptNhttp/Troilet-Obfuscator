using System;
using System.Text;

namespace TroiletCore.Plugin
{
    public abstract class PluginBase
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }
        public abstract Version Version { get; }
        public abstract PluginConfigBase? Config { get; protected set; }

        public abstract void OnLoad();
        public virtual void OnUnload() {}

        public string ToString(bool simple)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            if (this is IObfuscatorPlugin)
            {
                IObfuscatorPlugin op = (IObfuscatorPlugin)this;
                string? names = null;
                if (op.ShortNames != null)
                    for (int i = 0; i < op.ShortNames.Length; i++)
                        names += op.ShortNames[i] + (i + 1 == op.ShortNames.Length ? "" : ", ");

                sb.Append($" for {op.Platform} {(names == null ? "" : "(" + names + ")")}");
            }

            if (simple)
                return sb.ToString();

            sb.AppendLine();
            sb.AppendLine("  " + Description);
            sb.AppendLine($"  - Author: {Author}");
            sb.Append($"  - Version: {Version}");

            return sb.ToString();
        }
        public override string ToString() => ToString(true);
    }
}
