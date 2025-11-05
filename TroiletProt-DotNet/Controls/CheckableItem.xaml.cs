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
        private TreeViewItem? _Parent = null;
        private TreeViewItem _Item;

        public CheckableItem(TreeViewItem? parent, ImageSource img, string name)
        {
            _Parent = parent;

            InitializeComponent();

            itemname.Text = name;
            itemimage.Source = img;

            _Item = new TreeViewItem();
            _Item.Header = this;
            
            if (_Parent != null)
                _Parent.Items.Add(_Item);
        }
        public CheckableItem(CheckableItem parent, ImageSource img, string name) : this(parent._Item, img, name) {}

        public CheckableItem(ImageSource img, string name) : this((TreeViewItem?)null, img, name) {}

        public TreeViewItem GetItem(bool expanded = false)
        {
            _Item.IsExpanded = expanded;
            return _Item;
        }
    }
}
