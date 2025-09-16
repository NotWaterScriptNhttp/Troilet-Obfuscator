using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

using TroiletCore.Plugin;

namespace TroiletGUI.Pages
{
    /// <summary>
    /// Interakční logika pro SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public static SettingsPage Instance { get; private set; }
        public static PluginBase? SelectedProtector { get; private set; }

        public void SetObfuscator(PluginBase pb)
        {
            if (!(pb is IObfuscatorPlugin))
                return;

            SelectedProtector = pb;
            ObfuscatorPicker.SelectedItem = pb;
        }

        public SettingsPage()
        {
            Instance = this;

            InitializeComponent();

            ObfuscatorPicker.ItemsSource = PluginManager.Obfuscators;
        }

        private void ObfuscatorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedProtector = (PluginBase?)ObfuscatorPicker.SelectedItem;
        }
    }
}
