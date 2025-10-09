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

namespace TroiletGUI.Controls
{
    /// <summary>
    /// Interakční logika pro RangeSetting.xaml
    /// </summary>
    public partial class RangeSetting : UserControl
    {
        public double Min
        {
            get => _range.Minimum;
            set => _range.Minimum = value;
        }
        public double Max
        {
            get => _range.Maximum;
            set => _range.Maximum = value;
        }
        public double Value
        {
            get => _range.Value;
            set
            {
                _range.Value = value;
                _value.Content = value.ToString();
            }
        }

        public RangeSetting()
        {
            InitializeComponent();

            _range.ValueChanged += (s, e) => _value.Content = _range.Value.ToString();
        }
    }
}
