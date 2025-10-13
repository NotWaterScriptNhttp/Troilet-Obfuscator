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
        private PSettingSection? _SelectedSection = null;

        public static SettingsPage Instance { get; private set; }
        public static PluginBase? SelectedProtector { get; private set; }

        private void AddSettings(PSettingSection? sec)
        {
            settingItems.Children.Clear();

            if (sec == null)
                return;
            foreach (KeyValuePair<string, PluginSetting> kvp in sec.Settings)
            {
                PluginSetting s = kvp.Value;
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
                            l.Content = ls.Label;
                            l.FontSize = 18.0d;
                            l.FontWeight = FontWeight.FromOpenTypeWeight(700);
                            l.Foreground = Globals.WHITE;

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
                            cb.Content = ps.Label;
                            cb.IsChecked = ts.Value;
                            cb.Foreground = Globals.WHITE;
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
                            sl.Text = rs.Label;
                            sl.Value = rs.Value;
                            sl.Min = rs.Range.Start;
                            sl.Max = rs.Range.End;
                            sl.Step = rs.Step;
                            sl.Foreground = Globals.WHITE;
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
            }
        }

        private void ObfuscatorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => SetObfuscator((PluginBase?)ObfuscatorPicker.SelectedItem);

        public SettingsPage()
        {
            Instance = this;

            InitializeComponent();

            ObfuscatorPicker.ItemsSource = PluginManager.Obfuscators;
        }

        public void SetObfuscator(PluginBase? pb)
        {
            if (pb == null || !(pb is IObfuscatorPlugin op))
                return;

            SelectedProtector = pb;
            ObfuscatorPicker.SelectedItem = pb;

            settingTabs.Children.Clear();
            settingItems.Children.Clear();
            _SelectedSection = null;
            if (pb.Config == null)
                return;

            foreach (PSettingSection s in pb.Config.Sections)
            {
                var sec = new SettingSection(s);
                sec.Content = s.Name;
                sec.MouseDown += (se, e) =>
                {
                    foreach (SettingSection ss in settingTabs.Children)
                        ss.IsSelected = false;

                    _SelectedSection = s;
                    sec.IsSelected = true;
                };

                if (_SelectedSection == null)
                {
                    sec.IsSelected = true;
                    _SelectedSection = s;
                }

                settingTabs.Children.Add(sec);
            }

            AddSettings(_SelectedSection);
        }
    }
}
