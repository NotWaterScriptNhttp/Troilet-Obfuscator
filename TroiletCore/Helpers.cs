using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace TroiletCore
{
    internal class Helpers
    {
        private static List<string> TempFiles = new List<string>();
        private static string AssemblyName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().ManifestModule.Name);

        public static Random random = new Random();

        public static byte[] GetResource(string resource)
        {
            MemoryStream ms = new MemoryStream();
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"{AssemblyName}.Resources.{resource}").CopyTo(ms);

            return ms.ToArray();
        }
        public static byte[] GenerateBytes(int len = 16)
        {
            byte[] bytes = new byte[len];
            random.NextBytes(bytes);
            return bytes;
        }
        public static void RunApp(string filename, Action<DataReceivedEventArgs> output, Action<DataReceivedEventArgs> error, string args = "")
        {
            Process proc = new Process()
            {
                StartInfo =
                {
                    FileName = filename,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            proc.OutputDataReceived += (s, e) =>
            {
                Console.WriteLine($"Message: {e.Data}");
                output(e);
            };
            proc.ErrorDataReceived += (s, e) =>
            {
                Console.WriteLine($"Error: {e.Data}");
                error(e);
            };

            proc.Start();

            proc.WaitForExit();
        }
        public static string GetFreeTempFilename(string ext, string filename = "", bool useGUID = true)
        {
            string pre = (useGUID ? filename + "_" : filename);

            if (!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");

            string file = Path.GetFullPath(Path.Combine("Temp", $"{pre}{(useGUID ? new Guid(GenerateBytes()).ToString() : "")}.{ext}"));

            TempFiles.Add(file);

            return file;
        }
        public static string RandomString(int length, int offset = -1)
        {
            if (offset < length)
                offset = length;

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, random.Next(length, offset))
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static void DeleteTempfiles() => TempFiles.ForEach(file =>
        {
            try
            {
                File.Delete(file);
            }
            catch { }
        });
    }
}
