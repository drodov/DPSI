using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ImageHelper
{
    public static class ImageConverter
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

        public static BitmapImage GetGreyImage(BitmapImage sourceBitmap)
        {
            int height = sourceBitmap.PixelHeight;
            int width = sourceBitmap.PixelWidth;

            Bitmap bitmap = BitmapImage2Bitmap(sourceBitmap);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var hsv = new HSV(bitmap.GetPixel(j, i));
                    bitmap.SetPixel(j, i, (new HSV(0, 0, hsv.Value)).ToColor());
                }
            }
            return Bitmap2BitmapImage(bitmap);
        }

        public static BitmapImage GetBinaryImage(BitmapImage sourceBitmap)
        {
            int height = sourceBitmap.PixelHeight;
            int width = sourceBitmap.PixelWidth;

            var brightnessesNumber = new int[256];
            int pixelsNumber = height*width;

            Bitmap bitmap = BitmapImage2Bitmap(sourceBitmap);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var hsv = new HSV(bitmap.GetPixel(j, i));
                    brightnessesNumber[hsv.Value]++;
                }
            }

            int treshhold = brightnessesNumber.Select((t, i) => i*t).Sum() / pixelsNumber;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var hsv = new HSV(bitmap.GetPixel(j, i));
                    bitmap.SetPixel(j, i, (new HSV(0, 0, hsv.Value >= treshhold ? (byte)255 : (byte)0)).ToColor());
                }
            }

            return Bitmap2BitmapImage(bitmap);
        }
    }
}
