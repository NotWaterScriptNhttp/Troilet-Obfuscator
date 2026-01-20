using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TroiletCore;

namespace TroiletProt_DotNet.Controls
{
    [Flags]
    internal enum DNImage
    {
        Class = 0,
        Interface = 1,
        Module = 2,
        Namespace = 4,
        Event = 8,
        Field = 16,
        Method = 32,
        Property = 64,
        Add = 128,
        Remove = 256,
        Enum = 512,

        Private = 1024,
        Protected = 2048
    }

    internal static class DNImageCache
    {
        public static Dictionary<DNImage, ImageSource> CachedImage = new Dictionary<DNImage, ImageSource>();
    
        private static void LoadImage(DNImage img, string name)
        {
            byte[]? data = Globals.ReadEmbed(name + ".png");
            if (data == null)
                throw new ArgumentNullException("name", "Cannot be found.");

            CachedImage[img] = new PngBitmapDecoder(new MemoryStream(data), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
        }
        private static void LoadBundle(DNImage imgtype, string name)
        {
            LoadImage(imgtype, name);
            LoadImage(imgtype | DNImage.Private, name + "Private");
            LoadImage(imgtype | DNImage.Protected, name + "Protected");
        }

        public static void InitImages()
        {
            if (CachedImage.Count > 0)
                return;

            LoadImage(DNImage.Class, "Class");
            LoadImage(DNImage.Interface, "Interface");
            LoadImage(DNImage.Module, "Module");
            LoadImage(DNImage.Namespace, "Namespace");
            LoadBundle(DNImage.Event, "Event");
            LoadBundle(DNImage.Field, "Field");
            LoadBundle(DNImage.Property, "Property");
            LoadBundle(DNImage.Method, "Method");
            LoadImage(DNImage.Add, "Add");
            LoadImage(DNImage.Remove, "Remove");
            LoadImage(DNImage.Enum, "Enumeration");
        }

        public static ImageSource? GetImage(DNImage img)
        {
            if (CachedImage.TryGetValue(img, out var cimg))
                return cimg;

            return null;
        }
    }
}
