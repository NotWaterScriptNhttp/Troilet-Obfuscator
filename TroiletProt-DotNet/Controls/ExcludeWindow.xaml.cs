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
            wndclosebtn.MakeButton((s, e) => Hide());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            void AddClass(ExclusionItemBase parent, TypeDef t)
            {
                DNImage timg = DNImage.Class;
                if (t.IsEnum)
                    timg = DNImage.Enum;
                else if (t.IsInterface)
                    timg = DNImage.Interface;

                ClassItem tItem = new ClassItem(parent, DNImageCache.GetImage(timg), t.Name);
                foreach (EventDef ev in t.Events)
                    new CheckableItem(tItem, DNImageCache.GetImage(DNImage.Event), ev.Name);
                foreach (FieldDef fld in t.Fields)
                    new CheckableItem(tItem, DNImageCache.GetImage(DNImage.Field), fld.Name);
                foreach (PropertyDef prop in t.Properties)
                    new CheckableItem(tItem, DNImageCache.GetImage(DNImage.Property), prop.Name);
                foreach (MethodDef m in t.Methods)
                    new CheckableItem(tItem, DNImageCache.GetImage(DNImage.Method), m.Name);
                foreach (TypeDef n in t.NestedTypes)
                    AddClass(tItem, n);
            }

            if (explorer.HasItems)
                return;

            DNImageCache.InitImages();

            foreach (var mdl in Asm.Modules)
            {
                //CheckableItem mdlItem = new CheckableItem(DNImageCache.GetImage(DNImage.Module), mdl.Name);
                CheckableItem mdlItem = new CheckableItem(DNImageCache.GetImage(DNImage.Module), mdl.Name);
                Dictionary<string, CheckableItem> nsItems = new Dictionary<string, CheckableItem>();
                foreach (TypeDef t in mdl.Types)
                {
                    string ns = t.Namespace;
                    if (string.IsNullOrEmpty(ns))
                        ns = "-";

                    CheckableItem nsItem;
                    if (!nsItems.TryGetValue(ns, out nsItem))
                    {
                        nsItem = new CheckableItem(mdlItem, DNImageCache.GetImage(DNImage.Namespace), ns);
                        nsItems.Add(ns, nsItem);
                    }

                    AddClass(nsItem, t);
                }

                explorer.Items.Add(mdlItem.GetItem(true));
            }
        }
    }
}
