using System;

namespace TroiletCore
{
    public sealed class TroiletConfig
    {
        public string Root = Environment.CurrentDirectory;

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
        }
    }
}
