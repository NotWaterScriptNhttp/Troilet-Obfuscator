using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroiletCore.Plugins
{
    public class PluginLogger
    {
        private PluginBase plugin;
        internal Action<string> LogMethod = Console.WriteLine;

        internal PluginLogger(PluginBase plugin) => this.plugin = plugin;

        public void Log(string message, ConsoleColor col = ConsoleColor.White)
        {
            Console.ForegroundColor = col;
            LogMethod($"[{plugin.Name}]: {message}");
            Console.ResetColor();
        }
        internal void Log(string message, ConsoleColor col = ConsoleColor.White, string prefix = "")
        {
            Console.ForegroundColor = col;
            LogMethod($"[{plugin.Name} {prefix}]: {message}");
            Console.ResetColor();
        }

        public void Info(string message) => Log(message, ConsoleColor.Blue, "INFO");
        public void Warn(string message) => Log(message, ConsoleColor.Yellow, "WARN");
        public void Error(string message) => Log(message, ConsoleColor.Red, "ERROR");
    }
}
