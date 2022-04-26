using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TroiletCore.Plugins;

namespace TroiletTest_Plugin
{
    public class Main : PluginBase
    {
        public override string Name { get; protected set; } = "Test Plugin";
        public override string Author { get; protected set; } = "NoobSlayer";
        public override string Version { get; protected set; } = "0.0.1";
        public override string Description { get; protected set; } = "A test plugin made for Troilet";

        public override void OnDisable()
        {
            Logger.Info("Plugin disabled");
        }
        public override bool OnEnable()
        {
            Logger.Info("Plugin enabled");
            return true;
        }

        public override void OnCreate()
        {
            Logger.Warn("Hello, world!");
        }

        public override int OnInt32(int i32)
        {
            return i32 - 10;
        }
    }
}
