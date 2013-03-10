using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace BitmapHelper
{
    public static class BitmapConverter
    {
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return new Bitmap(bitmap);
        }

        public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage;
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }
    }
}
