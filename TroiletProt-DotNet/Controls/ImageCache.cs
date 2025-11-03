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

        Private = 128,
        Protected = 256
    }

    internal static class DNImageCache
    {
        public static Dictionary<DNImage, ImageSource> CachedImage = new Dictionary<DNImage, ImageSource>();
    
        private static void LoadImage(DNImage img, string name)
        {
            Stream? s = Utils.GetResourceStream(name + ".png");
            if (s == null)
                throw new ArgumentNullException("name", "Cannot be found.");

            CachedImage[img] = new PngBitmapDecoder(s, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
        }
        private static void LoadBundle(DNImage imgtype, string name)
        {

        }

        public static void InitImages()
        {

        }
    }
}
