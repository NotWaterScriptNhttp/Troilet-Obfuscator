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
        public delegate void OnChange(object sender, RoutedPropertyChangedEventArgs<double> e);

        public object Text
        {
            get => _title.Content;
            set => _title.Content = value;
        }
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

        public double Step
        {
            get => _range.TickFrequency;
            set => _range.TickFrequency = value;
        }

        public new Brush Foreground
        {
            set
            {
                _title.Foreground = value;
                _value.Foreground = value;
            }
        }

        public event OnChange? ValueChanged;

        public RangeSetting()
        {
            InitializeComponent();

            _range.IsSnapToTickEnabled = true;
            _range.ValueChanged += (s, e) =>
            {
                _value.Content = _range.Value.ToString();
                if (ValueChanged != null)
                    ValueChanged.Invoke(s, e);
            };
        }
    }
}
