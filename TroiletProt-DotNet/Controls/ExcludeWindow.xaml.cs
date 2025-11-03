using dnlib.DotNet;
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

using TroiletProt_DotNet.Extensions;

namespace TroiletProt_DotNet.Controls
{
    /// <summary>
    /// Interakční logika pro ExcludeWindow.xaml
    /// </summary>
    public partial class ExcludeWindow : Window
    {
        public static ExcludeWindow? Instance { get; internal set; } = null;

        private AssemblyDef Asm;

        #region Events

        private void Window_Activated(object sender, EventArgs e)
        {
            BorderThickness = new Thickness(1, 1, 1, 1);
        }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            BorderThickness = new Thickness(0, 0, 0, 0);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Normal;
            DragMove();
        }

        #endregion

        public ExcludeWindow(AssemblyDef asm)
        {
            Asm = asm;

            InitializeComponent();

            wndname.Content = asm.Name;
            wndclosebtn.MakeButton((s, e) => Close());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (explorer.HasItems)
                return;

            foreach (var mdl in Asm.Modules)
            {
                TreeViewItem mdlItem = new TreeViewItem();
                mdlItem.Header = new CheckableItem(null, mdl.Name);

                explorer.Items.Add(mdlItem);
            }
        }
    }
}
