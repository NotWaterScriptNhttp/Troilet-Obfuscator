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
    /// Interakční logika pro ClassItem.xaml
    /// </summary>
    public partial class ClassItem : ExclusionItemBase
    {
        public bool IsClassRedacted { get; private set; }

        public ClassItem(ImageSource img, string name) => Setup(img, name);
        public ClassItem(TreeViewItem? parent, ImageSource img, string name) => Setup(parent, img, name);
        public ClassItem(ExclusionItemBase parent, ImageSource img, string name) => Setup(parent, img, name);

        public override void Setup(TreeViewItem? parent, ImageSource img, string name)
        {
            InitializeComponent();
            base.Setup(parent, img, name);

            itemname.Text = name;
            itemimage.Source = img;

            itemexclude.Checked += (s, e) =>
            {
                Console.WriteLine("Checked");
            };
            itemexclude.Unchecked += (s, e) =>
            {
                Console.WriteLine("Unchecked");
            };
        }

        private void itemname_MouseEnter(object sender, MouseEventArgs e)
        {
            itemname.Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160));
        }
        private void itemname_MouseLeave(object sender, MouseEventArgs e)
        {
            itemname.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }

        private void itemname_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsClassRedacted = !IsClassRedacted;
            if (IsClassRedacted)
                itemname.TextDecorations = TextDecorations.Strikethrough;
            else itemname.TextDecorations = null;
            e.Handled = true;
        }
    }
}
