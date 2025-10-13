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

namespace TroiletGUI.Controls
{
    /// <summary>
    /// Interakční logika pro SettingSection.xaml
    /// </summary>
    public partial class SettingSection : UserControl
    {
        private bool _selected = false;

        public PSettingSection Section { get; private set; }
        public bool IsSelected
        {
            get => _selected;
            set
            {
                if (value)
                    _selector.Fill = Globals.WHITE;
                else _selector.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                _selected = value;
            }
        }
        public new object Content
        {
            get => _content.Content;
            set => _content.Content = value;
        }

        public SettingSection(PSettingSection sec)
        {
            Section = sec;
            InitializeComponent();
        }
    }
}
