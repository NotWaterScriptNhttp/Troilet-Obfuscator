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
using System.Windows.Shapes;

namespace TroiletProt_DotNet.Controls
{
    /// <summary>
    /// Interakční logika pro ExcludeWindow.xaml
    /// </summary>
    public partial class ExcludeWindow : Window
    {
        public static ExcludeWindow? Instance { get; private set; } = null;

        public ExcludeWindow()
        {
            InitializeComponent();
        }
    }
}
