using Newtonsoft.Json;
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
using TroiletGUI.Controls;

namespace TroiletGUI.Pages
{
    public partial class SettingsPage : Page
    {
        private static readonly SolidColorBrush WHITE = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        public static SettingsPage Instance { get; private set; }
        public static PluginBase? SelectedProtector { get; private set; }

        public void SetObfuscator(PluginBase? pb)
        {
            if (pb == null || !(pb is IObfuscatorPlugin op))
                return;

            SelectedProtector = pb;
            ObfuscatorPicker.SelectedItem = pb;

            settingItems.Children.Clear();
            if (pb.Config == null)
                return;

            string json = pb.Config.SaveAsJSON();
            Console.WriteLine(json);
            /*if (pb != null && pb.Config != null)
                foreach (object s in pb.Config.Settings)
                {
                    if (s is not PluginSetting ps)
                        return;

                    string name = ps.Name.ToLower().Replace(" ", "_");
                    switch (ps.Type)
                    {
                        case PluginSettingType.Label:
                            {
                                PSettingLabel? ls = ps as PSettingLabel;
                                if (ls == null)
                                    break;

                                Label l = new Label();
                                l.Name = name;
                                l.Content = ls.Value;
                                l.FontSize = 18.0d;
                                l.FontWeight = FontWeight.FromOpenTypeWeight(700);
                                l.Foreground = WHITE;

                                settingItems.Children.Add(l);
                                break;
                            }
                        case PluginSettingType.Toggle:
                            {
                                PSettingToggle? ts = ps as PSettingToggle;
                                if (ts == null)
                                    break;

                                CheckBox cb = new CheckBox();
                                cb.Name = name;
                                cb.Content = ps.Name;
                                cb.IsChecked = ts.Value;
                                cb.Foreground = WHITE;
                                cb.Checked += (s, e) => ts.Value = true;
                                cb.Unchecked += (s, e) => ts.Value = false;

                                settingItems.Children.Add(cb);
                                break;
                            }
                        case PluginSettingType.Text:
                            {
                                PSettingText? ts = ps as PSettingText;
                                if (ts == null)
                                    break;

                                TextBox tb = new TextBox();
                                tb.Name = name;
                                tb.Text = ts.Value;
                                tb.TextChanged += (s, e) => ts.Value = tb.Text;

                                settingItems.Children.Add(tb);
                                break;
                            }
                        case PluginSettingType.Range:
                            {
                                PSettingRange? rs = ps as PSettingRange;
                                if (rs == null)
                                    continue;

                                RangeSetting sl = new RangeSetting();
                                sl.Name = name;
                                sl.Text = rs.Name;
                                sl.Value = rs.Value;
                                sl.Min = rs.Range.Start;
                                sl.Max = rs.Range.End;
                                sl.Step = rs.Step;
                                sl.Foreground = WHITE;
                                sl.ValueChanged += (s, e) => rs.Value = sl.Value;

                                settingItems.Children.Add(sl);
                                break;
                            }
                        case PluginSettingType.Combo:
                            {
                                PSettingCombo? cs = ps as PSettingCombo;
                                if (cs == null)
                                    break;

                                ComboBox cb = new ComboBox();
                                cb.Name = name;
                                cb.ItemsSource = cs.Options;
                                cb.SelectedItem = cs.Value;
                                cb.SelectionChanged += (s, e) => cs.Value = cb.SelectedItem;

                                settingItems.Children.Add(cb);
                                break;
                            }
                    }
                }*/
        }

        public SettingsPage()
        {
            Instance = this;

            InitializeComponent();

            ObfuscatorPicker.ItemsSource = PluginManager.Obfuscators;
        }

        private void ObfuscatorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => SetObfuscator((PluginBase?)ObfuscatorPicker.SelectedItem);
    }
}
