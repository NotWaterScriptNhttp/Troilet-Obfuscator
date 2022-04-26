using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Toolkit.Uwp.Notifications;

using TroiletCore;
using TroiletCore.Plugins;

namespace TroiletGUI
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

    public partial class MainWindow : Window
    {
        //Types
        private struct Script
        {
            public string path;
            public bool ShouldBeObfuscated;
        }

        //Variables
        private Obfuscator _obfuscator;

        private List<Script> rawScripts = new List<Script>();
        private List<Script> scriptsToObf
        {
            get
            {
                return rawScripts.Where(scr => scr.ShouldBeObfuscated).ToList();
            }
        }

        //Private methods
        private static void Delay(int millis, Action action)
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = millis;
            timer.Enabled = true;
            timer.Start();

            timer.Tick += (s, e) =>
            {
                timer.Enabled = false;
                timer.Stop();

                action();
            };
        }

        private byte[] ReadResource(string resourceName)
        {
            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream($"TroiletGUI.Resources.{resourceName}"); // Get the resource stream
            MemoryStream ms = new MemoryStream(); // create a new MemoryStream so we can return the Stream as a byte[]
            s.CopyTo(ms); // copy the stream to memorystream

            s.Dispose(); // clear the stream

            return ms.ToArray(); // return the memorystream byte[]
        }
        private void _obfuscator_OnStatusUpdate(ObfuscatorStatus status)
        {
            this.Title = "Troilet Obfuscator GUI | Status: " + status.ToString();

            if (status == ObfuscatorStatus.Finished)
                Delay(2500, () => this.Title = "Troilet Obfuscator GUI");
        }

        #region App window callbacks
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PluginManager pManager = new PluginManager(); // Create PluginManager

            pManager.LogMethod = (s) =>
            {
                Output.AppendText(s + "\n"); // Adds a new line and writes the text s
            };

            pManager.LoadPlugins(false); // Loads the plugins in "Plugins"

            foreach (PluginBase pb in pManager.LoadedPlugins) // Add loaded plugins into LoadedPlugins listbox
                LoadedPlugins.Items.Add(pb.Name); // Adds the plugin name into the listbox

            _obfuscator = new Obfuscator(pManager); // Create an instance of obfuscator
            _obfuscator.OnStatusUpdate += _obfuscator_OnStatusUpdate; // add a callback to OnStatusUpdate

            _obfuscator.Setup(); //Creates the required files for the obfuscator
        }
        private void PluginToggle_Click(object sender, RoutedEventArgs e)
        {
            void UpdateLists() // Updates the lists
            {
                LoadedPlugins.Items.Clear();
                EnabledPlugins.Items.Clear();

                foreach (PluginBase pb in _obfuscator.PluginManager.LoadedPlugins)
                {
                    if (pb.IsEnabled)
                        EnabledPlugins.Items.Add(pb.Name);
                    else LoadedPlugins.Items.Add(pb.Name);
                }
            }

            ListBox lb = null; // Create a ListBox var so we don't need to dupe code

            if (LoadedPlugins.SelectedIndex >= 0) // Check if something is selected
                lb = LoadedPlugins;
            else if (EnabledPlugins.SelectedIndex >= 0) // Check if something is selected
                lb = EnabledPlugins;

            if (lb == null) // Check if there is any ListBox thats been selected
                return;

            _obfuscator.PluginManager.Toggle((string)lb.SelectedItem); // Toggles the Plugin
            UpdateLists(); // Updates the boxes
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists("Scripts")) // checks if the directory "Scripts" exists
                Directory.CreateDirectory("Scripts"); // create dir "Scripts"

            if (rawScripts != null) // check if rawScripts is created
                rawScripts.Clear();

            if (RawScrs != null) // check if RawScriptsListBox is created
                RawScrs.Items.Clear();
            if (ScrsToObfuscate != null) // check if ScriptsToObfuscateListBox is created
                ScrsToObfuscate.Items.Clear();

            foreach (string file in Directory.GetFiles("Scripts")) // go through all the files in the "Scripts" dir
            {
                if (file.EndsWith(".lua") || file.EndsWith(".txt")) // check if file is ending with ".lua" or ".txt"
                {
                    rawScripts.Add(new Script
                    {
                        path = file,
                        ShouldBeObfuscated = false
                    }); // Add the new script to the script list
                    RawScrs.Items.Add(System.IO.Path.GetFileName(file)); // Add the new script to the listbox
                }
            }
        }
        private void ToggleScript_Click(object sender, RoutedEventArgs e)
        {
            void UpdateLists()
            {
                RawScrs.Items.Clear();
                ScrsToObfuscate.Items.Clear();

                foreach (Script scr in rawScripts)
                {
                    if (scr.ShouldBeObfuscated)
                        ScrsToObfuscate.Items.Add(System.IO.Path.GetFileName(scr.path));
                    else RawScrs.Items.Add(System.IO.Path.GetFileName(scr.path));
                }
            }

            ListBox lb = null; // Create a ListBox var so we don't need to dupe code

            if (RawScrs.SelectedIndex >= 0)
                lb = RawScrs;
            else if (ScrsToObfuscate.SelectedIndex >= 0)
                lb = ScrsToObfuscate;

            if (lb == null)
                return;

            //This is very ineffective
            for (int i = 0; i < rawScripts.Count; i++) // go throught all the scripts we found in "Scripts"
            {
                if (System.IO.Path.GetFileName(rawScripts[i].path) == (string)lb.SelectedItem) // Check if the scripts filename is matching with the selected item
                {
                    rawScripts[i] = new Script
                    {
                        path = rawScripts[i].path,
                        ShouldBeObfuscated = !rawScripts[i].ShouldBeObfuscated
                    }; // Create a new Script struct cause too lazy to fix an error
                }
            }
            UpdateLists(); // Update the list boxes
        }
        private void ObfuscateBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach(Script scr in scriptsToObf)
            {
                Tuple<bool, string> data = _obfuscator.Obfuscate(scr.path, new ObfuscatorSettings
                {
                    bAppearance = (BytecodeAppearance)Enum.Parse(typeof(BytecodeAppearance), BytecodeAppr.SelectedItem == null ? "Random" : BytecodeAppr.SelectedItem.ToString()),
                    SerializeDebug = SerDebugCheckbox.IsChecked.Value
                });
                new ToastContentBuilder()
                    .AddAppLogoOverride(new Uri("https://cdn.discordapp.com/attachments/826522145064615980/932415173174644796/Icon2_128.jpeg"))
                    .AddText("Troilet")
                    .AddText($"Your script {System.IO.Path.GetFileName(scr.path)} was {(data.Item1 ? "Successfully" : "Unsuccessfully")} obfuscated")
                    .Show();
            }
        }
        #endregion

        //Public methods
        public MainWindow()
        {
            // AppDomain things
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => // Catches the unhandled exceptions
                {
                    File.AppendAllText("Log.log", $"UnhandledException: {((Exception)e.ExceptionObject).Message} | IsTerminating: {e.IsTerminating}\n");
                };
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    return EmbeddedAssembly.Get(args.Name);
                };

                EmbeddedAssembly.Load(ReadResource("Dlls.Microsoft.Toolkit.Uwp.Notifications.dll"), "Microsoft.Toolkit.Uwp.Notifications.dll");
                EmbeddedAssembly.Load(ReadResource("Dlls.System.ValueTuple.dll"), "System.ValueTuple.dll");
            }

            InitializeComponent(); // WPF shit

            Refresh_Click(null, null); // Call refresh cause code duping is bad

            foreach(string s in Enum.GetNames(typeof(BytecodeAppearance))) // go through all the enum names in BytecodeAppearance
                BytecodeAppr.Items.Add(s); // Add the enum name to the dropdown

            // Handling ListBox.SelectionChanged
            {
                void UpdatePGInfo(int index)
                {
                    if (index < 0 || index > (_obfuscator.PluginManager.LoadedPlugins.Count - 1))
                        return;

                    PluginBase pb = _obfuscator.PluginManager.LoadedPlugins[index];

                    PGname.Content = $"Plugin Name: {pb.Name}";
                    PGauthor.Content = $"Author: {pb.Author}";
                    PGversion.Content = $"Version: {pb.Version}";
                    PGdescription.Text = pb.Description;
                }

                LoadedPlugins.SelectionChanged += (s, e) =>
                {
                    UpdatePGInfo(LoadedPlugins.SelectedIndex);
                    EnabledPlugins.UnselectAll();
                    PluginToggle.Content = "Enable";
                };
                EnabledPlugins.SelectionChanged += (s, e) =>
                {
                    UpdatePGInfo(EnabledPlugins.SelectedIndex);
                    LoadedPlugins.UnselectAll();
                    PluginToggle.Content = "Disable";
                };

                RawScrs.SelectionChanged += (s, e) =>
                {
                    ScrsToObfuscate.UnselectAll();
                    ToggleScript.Content = "Add to obfuscation";
                };
                ScrsToObfuscate.SelectionChanged += (s, e) =>
                {
                    RawScrs.UnselectAll();
                    ToggleScript.Content = "Remove from obfuscation";
                };
            }

            VersionLabel.Content = "Version: v" + typeof(MainWindow).Assembly.GetName().Version.ToString(); // Gets the TroiletGUI assembly version
        }
    }
}
