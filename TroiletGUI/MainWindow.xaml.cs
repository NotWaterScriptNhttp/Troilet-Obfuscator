using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TroiletGUI.Extensions;

namespace TroiletGUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            settingsbtn.MakeButton((s, e) =>
            {

            });
            wndclosebtn.MakeButton((s, e) =>
            {
                Close();
            });
        }

        private void window_Activated(object sender, EventArgs e)
        {
            BorderThickness = new Thickness(1, 1, 1, 1);
        }
        private void window_Deactivated(object sender, EventArgs e)
        {
            BorderThickness = new Thickness(0, 0, 0, 0);
        }

        private void topbar_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
    }
}