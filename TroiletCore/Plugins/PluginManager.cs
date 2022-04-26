using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TroiletCore.Plugins
{
    public class PluginManager
    {
        public const string PluginFolder = "Plugins";

        private Action<string> logMethod = Console.WriteLine;

        public List<PluginBase> LoadedPlugins { get; private set; }
        public List<PluginBase> GetEnabledPlugins
        {
            get
            {
                return LoadedPlugins.Where((x) => x.IsEnabled).ToList(); // returns the plugins that are enabled by the user
            }
        }

        public Action<string> LogMethod
        {
            get
            {
                return logMethod;
            }
            set
            {
                logMethod = value;

                if (LoadedPlugins != null)
                    LoadedPlugins.ForEach(pb => pb.Logger.LogMethod = value);
            }
        }


        public PluginManager()
        {
            if (!Directory.Exists(PluginFolder)) // Checks if the Folder "Plugins" exists
                Directory.CreateDirectory(PluginFolder); // Creates the folder if it doesn't exist
        }

        public void LoadPlugins(bool messagesAsLogs = true) // messageAsLogs if true it will output Messageboxes as console logs
        {
            LoadedPlugins = new List<PluginBase>(); // Creates a new instane of List<PluginBase>

            string[] excludedPlugins = null; // An array for excluded plugins/files that may be libraries for the other plugins

            if (File.Exists(PluginFolder + "/exclude.txt")) // Checks if it exists
                excludedPlugins = File.ReadAllLines(PluginFolder + "/exclude.txt"); // if it does read it

            string[] plugins = Directory.GetFiles(PluginFolder, "*.dll", SearchOption.TopDirectoryOnly); // Gets all the files that end with .dll in the dir "Plugins"

            LogMethod($"Found total of {plugins.Length} plugins"); // Logs the findings

            Type baseType = typeof(PluginBase); // Creates a var with PluginBase type so we don't need to do typeof(PluginBase) in the loop (cause that would be very cpu ineffective)

            foreach (string pluginDll in plugins) // loops throught all the .dll files
            {
                if (excludedPlugins != null && excludedPlugins.Contains(Path.GetFileName(pluginDll))) // checks if its excluded
                {
                    LogMethod($"Skipping file/plugin {Path.GetFileName(pluginDll)} due to it being excluded"); // outputs that its excluded
                    continue;
                }

                Assembly plugin = Assembly.LoadFrom(Path.GetFullPath(pluginDll)); // loading the .dll file/plugin

                Type main = null; // creates a blank var

                foreach(Type type in plugin.GetTypes()) // Get all types/classes in the Assembly
                {
                    if (type.Name == "Main" || (type.BaseType != null && type.BaseType == baseType)) // Check if its name is "Main" or if the classes baseType is PluginBase
                    {
                        main = type; // Set the blank var to the Main or PluginBase type in the Assembly
                        break;
                    }
                }
       
                if (main == null)
                {
                    if (messagesAsLogs)
                        Console.WriteLine($"[{Path.GetFileName(pluginDll)}] Could not find a class with the name of 'Main' please verify that it exists");
                    else MessageBox.Show("Could not find a class with the name of 'Main' please verify that it exists", $"PluginLoader: {Path.GetFileName(pluginDll)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                } 

                PluginBase pb = (PluginBase)Activator.CreateInstance(main, null); // Create an instance of the found main/plugin typein the assembly
                pb.Logger = new PluginLogger(pb); // Create a logger for the type
                pb.Logger.LogMethod = LogMethod;

                LogMethod($"Successfully loaded '{pb.Name}[{pb.Version}]' by {pb.Author}");

                pb.OnCreate(); // This is something like .ctor in C#
                LoadedPlugins.Add(pb); // add it to the loaded plugins
            }
        }
        public Dictionary<string, T> InvokeEnabled<T>(string method, T io)
        {
            Type pbase = typeof(PluginBase);
            MethodInfo mi = pbase.GetMethods().Where(m => m.Name.ToLower() == method.ToLower()).FirstOrDefault(); // find the method with name <method> in the plugin base type

            if (mi == null) // if the method doesn't exist
            {
                LogMethod($"[PluginLoader]: Function {method} was not found!");
                return null;
            }

            if (mi.ReturnType != typeof(T)) // if the return type is not T
            {
                LogMethod($"[PluginLoader]: Function {method} returns {mi.ReturnType.Name}, your return type {typeof(T).Name}");
                return null;
            }

            //            Return type
            //                 V 
            Dictionary<string, T> data = new Dictionary<string, T>(); // Create an output dict cause we can have more than 1 plugin loaded
            //           ^
            //      Plugin name

            foreach(PluginBase pb in GetEnabledPlugins) // loop throught the enabled plugin types
            {
                data.Add(
                    pb.Name, // Gets the plugin name
                    (T)mi.Invoke(pb, new object[]
                    {
                        io
                    }
                ) // Calls the method <method> selected by the user and returns <T>
                );
            }

            return data; // returns the data that the plugin/s returned
        }
        public Dictionary<string, T> InvokeEnabled<T>(string method, object[] args)
        {
            //Does the same thing as above method just with object[] as calling argument
            Type pbase = typeof(PluginBase);
            MethodInfo mi = pbase.GetMethods().Where(m => m.Name.ToLower() == method.ToLower()).FirstOrDefault();

            if (mi == null)
            {
                LogMethod($"[PluginLoader]: Function {method} was not found!");
                return null;
            }

            if (mi.ReturnType != typeof(T))
            {
                LogMethod($"[PluginLoader]: Function {method} returns {mi.ReturnType.Name}, your return type {typeof(T).Name}");
                return null;
            }

            Dictionary<string, T> data = new Dictionary<string, T>();

            foreach (PluginBase pb in GetEnabledPlugins)
            {
                data.Add(pb.Name, (T)mi.Invoke(pb, args));
            }

            return data;
        }

        public void Toggle(string plugin)
        {
            PluginBase pb = LoadedPlugins.Where(p => p.Name == plugin).FirstOrDefault(); // gets the plugin thats loaded with the name <plugin>

            if (pb.IsEnabled) // If its enabled disable it
                pb.Disable();
            else pb.Enable(); // If its disabled enabled it
        }
        public void Toggle(string plugin, bool force)
        {
            PluginBase pb = LoadedPlugins.Where(p => p.Name == plugin).FirstOrDefault(); // Get the loaded plugin with name <plugin>

            if (force) // if force is true enabled the plugin
                pb.Enable();
            else pb.Disable(); // if force is false disabled the plugin
        }
    }
}
