using System;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Collections.Generic;

using TroiletCore.VM;                               
using TroiletCore.Plugins;
using TroiletCore.Troilet_Bytecode;

using LuaLib;

namespace TroiletCore
{
    /* My class layout
     * -----------------------------------------------
     * First are types
     * Second Variables
     * Third Properties
     * Fourth Static Methods (of any protection level)
     * Fifth Private Methods
     * Sixth Public Methods
     */

    public enum ObfuscatorStatus
    {
        SetupCompleted,
        CheckingFile,
        Compiling,
        RunningPluginsPhase1,
        Serializing,
        CreatingValues,
        CreatingVOpcodes,
        GeneratingVM,
        RunningPluginsPhase2, // use this state or remove it
        Minifing,
        CleaningUp,
        Finished
    }

    public class Obfuscator
    {
        //Delegates
        public delegate void ObfuscatorStatusDelegate(ObfuscatorStatus status);

        //Events
        public event ObfuscatorStatusDelegate OnStatusUpdate;

        //Properties
        public PluginManager PluginManager { get; private set; }
        public string Filename { get; private set; }

        //Variables
        internal static Obfuscator CurrentObfuscator;
        private ObfuscatorSettings defaultSettings = new ObfuscatorSettings()
        {
            bAppearance = BytecodeAppearance.Random
        };

        //Private/Internal Methods
        internal void UpdateStatus(ObfuscatorStatus status, params object[] args)
        {
            if (OnStatusUpdate != null)
                OnStatusUpdate.Invoke(status);

            List<object> l = new List<object>()
            {
                status
            };
            l.AddRange(args);

            PluginManager.InvokeEnabled("OnObfStateUpdate", l.ToArray());
        }

        //Public Methods
        public Obfuscator(PluginManager PM)
        {
            PluginManager = PM;
        }

        public void Setup()
        {
            string[] args = Environment.GetCommandLineArgs();

            // we don't need to check any other arguments cause this is the only argument in the whole obfuscator
            if (args.Length >= 1 && args[0] != "-nojar")
            {
                RegistryKey view = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey jre = view.OpenSubKey("SOFTWARE\\JavaSoft\\Java Runtime Environment");

                if (jre == null)
                    jre = view.OpenSubKey("SOFTWARE\\JavaSoft\\JDK");

                if (jre == null)
                {
                    MessageBox.Show("Please install java", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new Exception("Java is not installed!");
                    Environment.Exit(0);
                }
            }

            if (!File.Exists("LuaLib.dll"))
                File.WriteAllBytes("LuaLib.dll", Helpers.GetResource("Dlls.LuaLib.dll"));

            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");

            if (!Directory.Exists("Data/Scripts"))
                Directory.CreateDirectory("Data/Scripts");

            //Writing Lua files
            {
                string[] luaFiles = new string[]
                {
                    "lua51.dll",
                    "luac.exe",
                    "luajit.exe",
                    "unluac.jar"
                };

                foreach (string luaFile in luaFiles)
                    File.WriteAllBytes($"Data/{luaFile}", Helpers.GetResource($"Lua.{luaFile}"));
            }

            //Writing Script files
            {
                string[] scriptFiles = new string[]
                {
                    "fs",
                    "init",
                    "llex",
                    "lparser",
                    "luasrcdiet",
                    "optlex",
                    "optparser",
                    "utils"
                };

                foreach (string scriptFile in scriptFiles)
                    File.WriteAllBytes($"Data/Scripts/{scriptFile}.lua", Helpers.GetResource($"Lua.Scripts.{scriptFile}.lua"));
            }

            PluginManager.LoadedPlugins.ForEach(pb => pb._obfuscator = this);

            UpdateStatus(ObfuscatorStatus.SetupCompleted);
        }

        public Tuple<bool, string> Obfuscate(string file, ObfuscatorSettings settings = null, string output = "Output")
        {
            string filename = Path.GetFileNameWithoutExtension(file);

            CurrentObfuscator = this;
            Filename = filename;

            // Doing stuff
            {
                if (settings == null) // check if we have supplied any settings
                    settings = defaultSettings; // if not set it to default

                if (!Directory.Exists(output))
                    Directory.CreateDirectory(output);
            }

            UpdateStatus(ObfuscatorStatus.CheckingFile); // Update the status

            // Checking
            {
                string checkFile = Helpers.GetFreeTempFilename("luac", "check"); // creates a new filename

                // Checking if the file is valid
                Helpers.RunApp("Data/luac.exe", (e) => {}, (e) => {}, $"-o \"{checkFile}\" \"{Path.GetFullPath(file)}\""); // Executes luac (lua compiler) with the input file and outputs the compiled bytes to the created temp file

                // This gets executed after the RunApp has finished
                if (!File.Exists(checkFile)) // check if the temp file is created - indicates if the file is valid
                    return new Tuple<bool, string>(false, $"File '{Path.GetFileName(file)}' has an error"); // return false for failed and error
            }

            string minified = Helpers.GetFreeTempFilename("lua", $"{filename}_minified");

            // Minifing
            {
                Helpers.RunApp("Data/luajit.exe", (e) => { }, (e) => { },
                $"Data/Scripts/luasrcdiet.lua --opt-whitespace --opt-emptylines --noopt-numbers --opt-locals --noopt-strings --opt-comments \"{Path.GetFullPath(file)}\" -o \"{minified}\"");

                if (!File.Exists(minified))
                    return new Tuple<bool, string>(false, "The minifier didn't output any file!"); // return false for failed and error
            }

            UpdateStatus(ObfuscatorStatus.Compiling); // Update the status

            string compiled = Helpers.GetFreeTempFilename("luac", $"{filename}_compiled");

            // Compiling
            {
                Helpers.RunApp("Data/luac.exe", (e) => {}, (e) => {}, $"-o \"{compiled}\" \"{minified}\"");

                if (!File.Exists(compiled))
                    return new Tuple<bool, string>(false, "Failed to compile");
            }

            Chunk chunk = Chunk.Load(compiled); // Using Lualib to read the compiled chunk

            UpdateStatus(ObfuscatorStatus.RunningPluginsPhase1, chunk); // Trigger plugins to do things
            UpdateStatus(ObfuscatorStatus.Serializing); // Update the status

            byte[] serialized = TroiletSerializer.Serialize(chunk.MainFunction, true, settings.SerializeDebug);
#if DEBUG
            File.WriteAllBytes($"Temp/{filename}.troilet_raw", Convert.FromBase64String(Encoding.UTF8.GetString(serialized)));
            File.WriteAllBytes($"Temp/{filename}.troilet_b64", serialized);
#endif

            string VM = CreateVM.GetVM(serialized, settings);
            string vmfile = Helpers.GetFreeTempFilename("lua", $"{filename}_vm");
            File.WriteAllText(vmfile, VM); // this is for debug only

            /*string cVM = Helpers.GetFreeTempFilename("luac", $"{filename}_vm_compiled");
            // Compiling VM
            {
                Helpers.RunApp("Data/luac.exe", (e) => { }, (e) => { }, $"-o \"{cVM}\" \"{vmfile}\"");

                if (!File.Exists(cVM))
                    return new Tuple<bool, string>(false, "Failed to compile VM");
            }

            //Chunk vm_chunk = Chunk.Load(cVM);*/


            string finalFile = Helpers.GetFreeTempFilename("lua", $"{filename}_obfuscated", false);

#if DEBUG
            File.Copy(Path.GetFullPath(vmfile), finalFile);
#else
            //Minify the vm file
            {
                Helpers.RunApp("Data/luajit.exe", (e) => {}, (e) => {},
                    $"Data/Scripts/luasrcdiet.lua --maximum --opt-entropy --opt-emptylines --opt-eols --opt-numbers --opt-whitespace --opt-locals \"{Path.GetFullPath(vmfile)}\" -o \"{finalFile}\"");

                if (!File.Exists(finalFile))
                    return new Tuple<bool, string>(false, "The minifier didn't output any file!"); // return false for failed and error
            }
            File.WriteAllText(finalFile, File.ReadAllText(finalFile).Replace("\n", " "));
#endif

            string finalOut = Path.GetFullPath(Path.Combine(output, Path.GetFileName(finalFile)));

            if (File.Exists(finalOut))
                File.Delete(finalOut);

            File.Move(finalFile, finalOut);

            UpdateStatus(ObfuscatorStatus.CleaningUp);
            Helpers.DeleteTempfiles(); // remove the temp files that we created in this session

            UpdateStatus(ObfuscatorStatus.Finished); // Update the status

            return new Tuple<bool, string>(true, finalOut); // return true for success and path to the obfed file
        }
    }
}
