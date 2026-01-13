using System;
using System.IO;

namespace TroiletCore
{
    public sealed class TroiletConfig
    {
        public string Root = Environment.CurrentDirectory;
        public Random Rand = new Random();

        public string PluginsDir => Path.Combine(Root, "plugins");
        public string DependencyDir => Path.Combine(Root, "deps");
        public string TempDir => Path.Combine(Root, "temp");

        private static TroiletConfig? _instance = null;
        public static TroiletConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TroiletConfig();

                return _instance;
            }
        }

        public TroiletConfig(string? root = null /*path*/)
        {
            if (_instance != null)
                throw new ApplicationException("Config already created!");

            _instance = this;

            if (root != null)
                Root = root;

            if (!Directory.Exists(PluginsDir))
                Directory.CreateDirectory(PluginsDir);
            if (!Directory.Exists(DependencyDir))
                Directory.CreateDirectory(DependencyDir);
            Directory.CreateDirectory(TempDir);
        }
        ~TroiletConfig()
        {
            if (Directory.Exists(TempDir))
                Directory.Delete(TempDir, true);
        }

        public string? GetDepFile(string file)
        {
            string f = Path.Combine(DependencyDir, file);
            if (!File.Exists(f))
                return null;

            return f;
        }
        public string CreateTempFile(string name, string ext = "")
        {
            int tries = 5; // Sets the maximum try count
        GENERATE_NAME:
            if (tries <= 0)
                return Path.Combine(TempDir, name + ext);

            uint num = (uint)Rand.NextInt64(0x10000000, uint.MaxValue);
            string file = Path.Combine(TempDir, $"name_{num:X8}{ext}");
            if (File.Exists(file))
            {
                tries--;
                goto GENERATE_NAME;
            }

            return file;
        }
    }
}
