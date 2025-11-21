using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace TroiletProt_DotNet.Controls
{
    public class ExclusionItemBase : UserControl
    {
        protected TreeViewItem? _Parent = null;

        protected string _Name = string.Empty;
        protected ImageSource? _Image = null;
        protected TreeViewItem _Item;

        public virtual void Setup(TreeViewItem? parent, ImageSource img, string name)
        {
            _Parent = parent;
            _Name = name;
            _Image = img;

            _Item = new TreeViewItem();
            _Item.Header = this;
            if (parent != null)
                parent.Items.Add(_Item);
        }
        public void Setup(ImageSource img, string name) => Setup((TreeViewItem?)null, img, name);
        public void Setup(ExclusionItemBase parent, ImageSource img, string name) => Setup(parent._Item, img, name);

        public TreeViewItem GetItem(bool expanded = false)
        {
            _Item.IsExpanded = expanded;
            return _Item;
        }
    }
}
