using System;

namespace TroiletCore.Plugin
{
    public interface IObfuscatorPlugin
    {
        public string Platform { get; protected set; }
        public string[] PlatformExt { get; protected set; }
        public string[]? ShortNames { get; protected set; }

        /// <summary>
        /// Gets the platform icon, the function should check if the file is valid for that platform.
        /// </summary>
        /// <param name="file">File to check</param>
        /// <returns>Stream of a PNG image, or null for not valid</returns>
        public Stream? GetPlatformIcon(byte[] fileData);
        public bool Obfuscate(string file, string output, string[]? deps = null);
    }
}
