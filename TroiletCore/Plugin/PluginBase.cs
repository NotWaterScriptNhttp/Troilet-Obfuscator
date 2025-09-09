using System;
using System.Text;

namespace TroiletCore.Plugin
{
    public abstract class PluginBase<T> where T : IPluginConfig
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }
        public abstract Version Version { get; }
        public abstract T? Config { get; protected set; }

        public abstract void OnLoad();
        public virtual void OnUnload() {}

        public override string ToString()
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

                sb.AppendLine($" for {op.Platform} {(names == null ? "" : "(" + names + ")")}");
            }
            else sb.AppendLine();

            sb.AppendLine("  " + Description);
            sb.AppendLine($"  - Author: {Author}");
            sb.Append($"  - Version: {Version}");

            return sb.ToString();
        }
    }
}
