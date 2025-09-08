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
    public partial class PluginPage : Page
    {
        public PluginPage()
        {
            InitializeComponent();

            foreach (var p in PluginManager.Plugins)
                loadedPlugins.Children.Add(new PluginItem(p.Name, p.Description));
        }
    }
}
