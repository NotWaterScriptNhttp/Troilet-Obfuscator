using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using TroiletCore;
using TroiletCore.Plugin;

using TroiletGUI.Pages;
using TroiletGUI.Extensions;

namespace TroiletGUI
{
    public partial class MainWindow : Window
    {
        private List<Button> _PageButtons = new List<Button>();

        private void AddPage(Button btn, Page p)
        {
            if (_PageButtons.Count <= 0)
                pageview.Content = p;

            btn.Click += (s, e) => pageview.Content = p;
            _PageButtons.Add(btn);
        }

        public MainWindow()
        {
            new TroiletConfig();
            PluginManager.Init();

            InitializeComponent();

            settingsbtn.MakeButton((s, e) =>
            {

            });
            wndclosebtn.MakeButton((s, e) => Environment.Exit(0));
            obfuscatebtn.MakeButton((s, e) =>
            {
                PluginBase? obf = SettingsPage.SelectedProtector;
                if (obf == null || obf is not IObfuscatorPlugin)
                    return;

                string outDir = System.IO.Path.Combine(Environment.CurrentDirectory, "output");
                Directory.CreateDirectory(outDir);

                string file = FilePage.SelectedFile;
                string output = System.IO.Path.Combine(outDir, System.IO.Path.GetFileName(file));
                if (((IObfuscatorPlugin)obf).Obfuscate(file, output, FilePage.Dependencies))
                    Console.WriteLine("Successfully obfusctated: {0}", System.IO.Path.GetFileName(file));
                else Console.WriteLine("Failed to obfusctate: {0}", System.IO.Path.GetFileName(file));
            });

            AddPage(sidefilebtn, new FilePage());
            AddPage(sidepluginsbtn, new PluginPage());
            AddPage(sidesettingsbtn, new SettingsPage());
        }

        private void window_Activated(object sender, EventArgs e)
        {
            BorderThickness = new Thickness(1, 1, 1, 1);
        }
        private void window_Deactivated(object sender, EventArgs e)
        {
            BorderThickness = new Thickness(0, 0, 0, 0);
        }

        private void topbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Normal;
            DragMove();
        }
    }
}