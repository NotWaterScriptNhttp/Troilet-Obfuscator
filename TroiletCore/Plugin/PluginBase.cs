using System;

namespace TroiletCore.Plugin
{
    public abstract class PluginBase
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }
        public abstract Version Version { get; }
    }
}
