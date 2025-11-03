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

namespace TroiletProt_DotNet.Controls
{
    /// <summary>
    /// Interakční logika pro NonCheckableItem.xaml
    /// </summary>
    public partial class CheckableItem : UserControl
    {
        public CheckableItem(ImageSource img, string name)
        {
            InitializeComponent();

            itemname.Text = name;
            itemimage.Source = img;
        }
    }
}
