using System;
using System.Collections.Generic;
using System.IO;
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

using Microsoft.Win32;
using TroiletCore;
using TroiletCore.Plugin;

namespace TroiletGUI.Pages
{
    public partial class FilePage : Page
    {
        private static List<string> _dependencies = new List<string>();

        public static string SelectedFile = string.Empty;
        public static string[] Dependencies => _dependencies.ToArray();

        private string? PickFile(string ext, bool multi = false)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = multi;
            dialog.CheckFileExists = true;
            dialog.ValidateNames = true;
            dialog.DefaultExt = ".*";

            if (dialog.ShowDialog() == true)
                return dialog.FileName;

            return null;
        }
        private ImageSource? GetIcon(string file)
        {
            string? ext = System.IO.Path.GetExtension(file);
            Stream? istream = null;
            if (ext == null)
                goto UNKNOWN_ICON;

            ext = ext.ToLower().Replace(".", "");

            foreach (var p in PluginManager.Obfuscators)
            {
                var op = p as IObfuscatorPlugin;
                if (op == null)
                    continue;

                bool isValidExt = false;
                foreach (string e in op.PlatformExt)
                    if (e.ToLower() == ext)
                    {
                        isValidExt = true; 
                        break;
                    }

                if (!isValidExt)
                    continue;
                if ((istream = op.GetPlatformIcon(file)) != null)
                    break;
            }

        UNKNOWN_ICON:
            if (istream == null && (istream = Utils.GetResourceStream("UnknownIcon.png")) == null)
                return null;

            return new PngBitmapDecoder(istream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
        }

        public FilePage()
        {
            InitializeComponent();

            fileIcon.Source = null;
            fileNameLbl.Content = "";
            fileSizeLbl.Content = "";
        }

        private void pickFileBtn_Click(object sender, RoutedEventArgs e)
        {
            string? nfile = PickFile(".*");
            if (nfile == null)
                return;

            SelectedFile = nfile;
            selectedFile.Text = SelectedFile;
            fileNameLbl.Content = System.IO.Path.GetFileName(SelectedFile);
            fileIcon.Source = GetIcon(nfile);
        }
    }
}
