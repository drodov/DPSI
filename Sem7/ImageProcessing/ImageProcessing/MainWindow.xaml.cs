using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ImageProcessing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ChartValue> SourceChartValues { get; set; }
        public ObservableCollection<ChartValue> ProcessedChartValues { get; set; }

        private BitmapImage _sourceBitmapImage;
        private readonly Settings _settings = new Settings
            {
                Fmin = 50,
                Gmin = 50,
                Fmax = 250,
                Gmax = 250
            };

        public MainWindow()
        {
            InitializeComponent();

            SourceChartValues = new ObservableCollection<ChartValue>();
            Chart1.DataContext = SourceChartValues;


            ProcessedChartValues = new ObservableCollection<ChartValue>();
            Chart2.DataContext = ProcessedChartValues;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = @"C:\Users\Dmitry\Pictures";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() != null)
            {
                try
                {
                    _sourceBitmapImage = new BitmapImage(new Uri(openFileDialog1.FileName));
                    Image1.Source = _sourceBitmapImage;

                    AddValuesToChart(SourceChartValues, _sourceBitmapImage);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void Preparation1_Click(object sender, RoutedEventArgs e)
        {
            double coeff = (_settings.Gmax - _settings.Gmin) / 255;
            int height = _sourceBitmapImage.PixelHeight;
            int width = _sourceBitmapImage.PixelWidth;

            Bitmap bitmap = BitmapImage2Bitmap(_sourceBitmapImage);
            for (int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    var hsv = new HSV(bitmap.GetPixel(j, i));
                    hsv.Value = hsv.Value * coeff + _settings.Gmin / 255;
                    bitmap.SetPixel(j, i, hsv.HSVToColor());
                }
            }

            bitmap.Save(@"D:\123.jpg");
            Image2.Source = Bitmap2BitmapImage(bitmap);

            AddValuesToChart(ProcessedChartValues, Image2.Source as BitmapImage);
        }

        private void Preparation2_Click(object sender, RoutedEventArgs e)
        {
            double coeff = (_settings.Fmax - _settings.Fmin) / 255;
            int height = _sourceBitmapImage.PixelHeight;
            int width = _sourceBitmapImage.PixelWidth;

            Bitmap bitmap = BitmapImage2Bitmap(_sourceBitmapImage);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var hsv = new HSV(bitmap.GetPixel(j, i));
                    if (hsv.Value < _settings.Fmin / 255)
                    {
                        hsv.Value = 0;
                    }
                    else if (hsv.Value > _settings.Fmax / 255)
                    {
                        hsv.Value = 1;
                    }
                    else
                    {
                        hsv.Value = (hsv.Value - _settings.Fmin / 255) / coeff;
                    }
                    bitmap.SetPixel(j, i, hsv.HSVToColor());
                }
            }

            bitmap.Save(@"D:\123.jpg");
            Image2.Source = Bitmap2BitmapImage(bitmap);

            AddValuesToChart(ProcessedChartValues, Image2.Source as BitmapImage);
        }

        private void SettingsItem_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_settings);
            settingsWindow.ShowDialog();
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return new Bitmap(bitmap);
        }

        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
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

        private void OpRoberts_Click(object sender, RoutedEventArgs e)
        {
            int height = _sourceBitmapImage.PixelHeight;
            int width = _sourceBitmapImage.PixelWidth;

            Bitmap bitmap = BitmapImage2Bitmap(_sourceBitmapImage);
            Bitmap newBitmap = BitmapImage2Bitmap(_sourceBitmapImage);
            for (int i = 0; i < height - 1; i++)
            {
                for (int j = 0; j < width - 1; j++)
                {
                    HSV hsv = GetRobertsPixel(new HSV(bitmap.GetPixel(j, i)), new HSV(bitmap.GetPixel(j + 1, i)),
                                              new HSV(bitmap.GetPixel(j, i + 1)), new HSV(bitmap.GetPixel(j + 1, i + 1)));
                    newBitmap.SetPixel(j, i, hsv.HSVToColor());
                }
            }

            newBitmap.Save(@"D:\123.jpg");
            Image2.Source = Bitmap2BitmapImage(newBitmap);

            AddValuesToChart(ProcessedChartValues, Image2.Source as BitmapImage);
        }

        private Color GetRobertsPixel(Color pxl0, Color pxl1, Color pxl2, Color pxl3)
        {
           // byte A = RobertsOperator(pxl0.A, pxl1.A, pxl2.A, pxl3.A);
            byte R = RobertsOperator(pxl0.R, pxl1.R, pxl2.R, pxl3.R);
            byte G = RobertsOperator(pxl0.G, pxl1.G, pxl2.G, pxl3.G);
            byte B = RobertsOperator(pxl0.B, pxl1.B, pxl2.B, pxl3.B);
            return Color.FromArgb( R, G, B);
        }

        private byte RobertsOperator(byte a, byte b, byte c, byte d)
        {
            return (byte) Math.Sqrt(Math.Pow(b - c, 2) + Math.Pow(a - d, 2));
        }

        private HSV GetRobertsPixel(HSV hsv0, HSV hsv1, HSV hsv2, HSV hsv3)
        {
            hsv0.Value = RobertsOperator(hsv0.Value, hsv1.Value, hsv2.Value, hsv3.Value);
            return hsv0;
        }

        private double RobertsOperator(double a, double b, double c, double d)
        {
            return Math.Sqrt(Math.Pow(b - c, 2) + Math.Pow(a - d, 2));
        }

        private void AddValuesToChart(ObservableCollection<ChartValue> chartValues, BitmapImage bitmapImage)
        {
            Bitmap bitmap = BitmapImage2Bitmap(bitmapImage);
            var mas = new int[256];
            int width = bitmapImage.PixelWidth;
            int height = bitmapImage.PixelHeight;
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    double val = new HSV(bitmap.GetPixel(j, i)).Value;
                    int idx = (int)(val*255);
                    mas[idx]++;
                }
            }

            chartValues.Clear();
            for (int i = 0; i < mas.Length; i++)
            {
                chartValues.Add(new ChartValue(i, mas[i]));
            }
        }
    }
}
