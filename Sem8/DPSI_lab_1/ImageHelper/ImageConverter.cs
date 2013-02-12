using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ImageHelper
{
    public static class ImageConverter
    {

        private static int _counter = 0;

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

            int treshhold = brightnessesNumber.Select((t, i) => i*t).Sum()/pixelsNumber;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var hsv = new HSV(bitmap.GetPixel(j, i));
                    bitmap.SetPixel(j, i, (new HSV(0, 0, hsv.Value >= treshhold ? (byte) 255 : (byte) 0)).ToColor());
                }
            }

            return Bitmap2BitmapImage(bitmap);
        }

        public static BitmapImage GetRecognizedImage(BitmapImage sourceBitmap)
        {
            Bitmap bitmap = BitmapImage2Bitmap(sourceBitmap);

            int[,] pixels = GetPixelValueArray(bitmap);

            var borders = new int[bitmap.Width, bitmap.Height];

            var labels = new int[bitmap.Width, bitmap.Height];
            int L = 1; // labels должна быть обнулена
            for (int y = 0; y < (int)bitmap.Height; y++)
            {
                for (int x = 0; x < (int)bitmap.Width; x++)
                {
                    Fill(bitmap, pixels, ref labels, ref borders, x, y, L++);
                }
            }

            DrawImage(bitmap, labels, borders);

            return Bitmap2BitmapImage(bitmap);
        }

        private static void Fill(Bitmap bitmap, int[,] pixels, ref int[,] labels, ref int[,] borders, int x, int y, int L)
        {
            _counter++;
            if (_counter < 6500)
            {
                if ((labels[x, y] == 0) && pixels[x, y] == 0)
                {
                    labels[x, y] = L;
                    if (x > 0)
                    {
                        Fill(bitmap, pixels, ref labels, ref borders, x - 1, y, L);
                    }
                    if (x > 0 && y > 0)
                    {
                        Fill(bitmap, pixels, ref labels, ref borders, x - 1, y - 1, L);
                    }
                    if (y > 0)
                    {
                        Fill(bitmap, pixels, ref labels, ref borders, x, y - 1, L);
                    }
                    if (x < bitmap.Width - 1 && y > 0)
                    {
                        Fill(bitmap, pixels, ref labels, ref borders, x + 1, y - 1, L);
                    }
                    if (x < bitmap.Width - 1)
                    {
                        Fill(bitmap, pixels, ref labels, ref borders, x + 1, y, L);
                    }
                    if (x < bitmap.Width - 1 && y < bitmap.Height - 1)
                    {
                        Fill(bitmap, pixels, ref labels, ref borders, x + 1, y + 1, L);
                    }
                    if (y < bitmap.Height - 1)
                    {
                        Fill(bitmap, pixels, ref labels, ref borders, x, y + 1, L);
                    }
                    if (x > 0 && y < bitmap.Height - 1)
                    {
                        Fill(bitmap, pixels, ref labels, ref borders, x - 1, y + 1, L);
                    }
                }
            }
            else
            {
                borders[x, y] = 1;
            }
            _counter--;
        }

        private static void DrawImage(Bitmap bitmap, int[,] labels, int[,] borders)
        {
            Dictionary<int, byte> colors = GetColors((int)bitmap.Width, (int)bitmap.Height, ref labels, borders);

            for (int y = 0; y < (int)bitmap.Height; y++)
            {
                for (int x = 0; x < (int)bitmap.Width; x++)
                {
                    bitmap.SetPixel(x, y, (new HSV(0, 0, colors[labels[x, y]])).ToColor());
                }
            }
        }

        private static Dictionary<int, byte> GetColors(int width, int height, ref int[,] labels, int[,] borders)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int basePoint = labels[x, y];

                    if (borders[x, y] == 1 && basePoint != 0)
                    {
                        int curPoint = 0;

                        if (x > 0)
                        {
                            curPoint = labels[x - 1, y];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(width, height, ref labels, basePoint, curPoint);
                                continue;
                            }
                        }
                        if (x > 0 && y > 0)
                        {
                            curPoint = labels[x - 1, y - 1];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(width, height, ref labels, basePoint, curPoint);
                                continue;
                            }
                        }
                        if (y > 0)
                        {
                            curPoint = labels[x, y - 1];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(width, height, ref labels, basePoint, curPoint);
                                continue;
                            }
                        }
                        if (x < width - 1 && y > 0)
                        {
                            curPoint = labels[x + 1, y - 1];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(width, height, ref labels, basePoint, curPoint);
                                continue;
                            }
                        }
                        if (x < width - 1)
                        {
                            curPoint = labels[x + 1, y];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(width, height, ref labels, basePoint, curPoint);
                                continue;
                            }
                        }
                        if (x < width - 1 && y < height - 1)
                        {
                            curPoint = labels[x + 1, y + 1];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(width, height, ref labels, basePoint, curPoint);
                                continue;
                            }
                        }
                        if (y < height - 1)
                        {
                            if (labels[x, y + 1] != 0 && labels[x, y + 1] != basePoint)
                            {
                                UniteColors(width, height, ref labels, basePoint, curPoint);
                                continue;
                            }
                        }
                        if (x > 0 && y < height - 1)
                        {
                            curPoint = labels[x - 1, y + 1];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(width, height, ref labels, basePoint, curPoint);
                                continue;
                            }
                        }
                    }
                }
            }

            var hashSet = new HashSet<int>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    hashSet.Add(labels[x, y]);
                }
            }

            var colors = new Dictionary<int, byte>();
            byte curColor = 0;
            for (int i = 0; i < hashSet.Count; i++)
            {
                int hashElement = hashSet.ElementAt(i);
                colors.Add(hashElement, (hashElement == 0) ? (byte)255 : curColor);
                curColor += 20;
                if (curColor > 230)
                {
                    curColor = 0;
                }
            }

            return colors;
        }

        private static void UniteColors(int width, int height, ref int [,] labels, int toChange, int changeBy)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (labels[x, y] == toChange)
                    {
                        labels[x, y] = changeBy;
                    }
                }
            }
        }

        private static int[,] GetPixelValueArray(Bitmap bitmap)
        {
            var pixels = new int[bitmap.Width, bitmap.Height];
            for (int y = 0; y < (int)bitmap.Height; y++)
            {
                for (int x = 0; x < (int)bitmap.Width; x++)
                {
                    pixels[x, y] = new HSV(bitmap.GetPixel(x, y)).Value;
                }
            }
            return pixels;
        }
    }
}