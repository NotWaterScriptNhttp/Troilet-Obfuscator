using System;

namespace TroiletCore.Plugin
{
    public interface IObfuscatorPlugin
    {
        public string Platform { get; protected set; }
        public string[] PlatformExt { get; protected set; }
        public string[]? ShortNames { get; protected set; }

        /// <summary>
        /// Loads the file to be obfuscated.
        /// </summary>
        /// <param name="file">File to check</param>
        /// <returns>Stream of a PNG image, or null for not valid</returns>
        public Stream? LoadFile(byte[] fileData);
        public bool Obfuscate(string file, string output, string[]? deps = null);
    }
}
