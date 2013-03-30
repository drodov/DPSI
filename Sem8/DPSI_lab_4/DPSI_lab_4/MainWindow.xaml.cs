using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BitmapHelper;
using Microsoft.Win32;

namespace DPSI_lab_4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double ALPHA = 0.5;
        private const double BETA = 0.5;
        private const double DLIMIT = 0.1;
        private const int NumberOfClasses = 3;
        private const int NumberOfImages = 9;
        private const int QSize = 128;
        private BitmapImage _sourceImage;
        private BitmapImage _noisedImage;
        private int _height;
        private int _width;
        private int _pixelsNumber;
        private int _noiseValue;
        private readonly BitmapImage[] _knowledgeImages = new BitmapImage[NumberOfImages];
        private readonly double[][] _knowledgeMatrix = new double[NumberOfImages][];
        private double[] X;
        private double[,] V;
        private double[] G;
        private double[] Q;
        private double[,] W;
        private double[] T;
        private double[] Y;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = @"C:\Users\Dmitry\Pictures\DPSI",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() != null)
            {
                try
                {
                    _sourceImage = new BitmapImage(new Uri(openFileDialog.FileName));
                    _noisedImage = _sourceImage;
                    SourceImage.Source = _sourceImage;
                    _width = _sourceImage.PixelWidth;
                    _height = _sourceImage.PixelHeight;
                    _pixelsNumber = _width * _height;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            const string baseUri = @"C:\Users\Dmitry\Pictures\DPSI\";
            _knowledgeImages[0] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "A")));
            Image1.Source = _knowledgeImages[0];

            _width = _knowledgeImages[0].PixelWidth;
            _height = _knowledgeImages[0].PixelHeight;
            _pixelsNumber = _width * _height;

            _knowledgeImages[1] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "A2")));
            Image2.Source = _knowledgeImages[1];
            _knowledgeImages[2] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "A3")));
            Image3.Source = _knowledgeImages[2];
            _knowledgeImages[3] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "C")));
            Image4.Source = _knowledgeImages[3];
            _knowledgeImages[4] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "C2")));
            Image5.Source = _knowledgeImages[4];
            _knowledgeImages[5] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "C3")));
            Image6.Source = _knowledgeImages[5];
            _knowledgeImages[6] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "K")));
            Image7.Source = _knowledgeImages[6];
            _knowledgeImages[7] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "K2")));
            Image8.Source = _knowledgeImages[7];
            _knowledgeImages[8] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "K3")));
            Image9.Source = _knowledgeImages[8];

            for (int i = 0; i < NumberOfImages; i++)
            {
                _knowledgeMatrix[i] = GetImageVector(BitmapConverter.BitmapImage2Bitmap(_knowledgeImages[i]));
            }

            var random = new Random();

            V = new double[_pixelsNumber, QSize];
            for (int i = 0; i < _pixelsNumber; i++)
            {
                for (int j = 0; j < QSize; j++)
                {
                    V[i, j] = 1 - 2 * random.NextDouble();
                }
            }

            G = new double[QSize];

            Q = new double[QSize];
            for (int i = 0; i < QSize; i++)
            {
                Q[i] = 1 - 2 * random.NextDouble();
            }

            W = new double[QSize, NumberOfClasses];
            for (int i = 0; i < QSize; i++)
            {
                for (int j = 0; j < NumberOfClasses; j++)
                {
                    W[i, j] = 1 - 2 * random.NextDouble();
                }
            }

            T = new double[NumberOfClasses];
            for (int i = 0; i < NumberOfClasses; i++)
            {
                T[i] = 1 - 2 * random.NextDouble();
            }

            Y = new double[NumberOfClasses];

            StartLearning();
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            NoiseValueTextBox.Text = (sender as Slider).Value.ToString();
        }

        private void NoiseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(NoiseValueTextBox.Text, out _noiseValue))
            {
                MessageBox.Show("Bad noise value!");
                return;
            }
            Bitmap bitmap = BitmapConverter.BitmapImage2Bitmap(_sourceImage);
            var random = new Random();
            var pointsHashSet = new HashSet<Point>();
            int pixelsToInvert = _pixelsNumber * _noiseValue / 100;
            for (int i = 0; i < pixelsToInvert; i++)
            {
                bool wasInverted = false;
                while (!wasInverted)
                {
                    Point point;
                    point.X = random.Next(0, _width);
                    point.Y = random.Next(0, _height);
                    if (pointsHashSet.Add(point))
                    {
                        Color curColor = bitmap.GetPixel(point.X, point.Y);
                        bitmap.SetPixel(point.X, point.Y, curColor.R > 230 ? Color.Black : Color.White);
                        wasInverted = true;
                    }
                }
            }
            _noisedImage = BitmapConverter.Bitmap2BitmapImage(bitmap);
            SourceImage.Source = _noisedImage;
        }

        private void RecognizeButton_Click(object sender, RoutedEventArgs e)
        {
            X = GetImageVector(BitmapConverter.BitmapImage2Bitmap(_noisedImage));
            CalculateG();
            CalculateY();
            Result1.Content = Y[0];
            Result4.Content = Y[1];
            Result7.Content = Y[2];

            for(int i = 0; i < Y.Length; i++)
            {
                if(Y[i] == Y.Max())
                {
                    MessageBox.Show("Class: " + (i + 1).ToString());
                }
            }
        }

        private void StartLearning()
        {
            bool flagToStopLearning = false;
            int iterator = 0;
            while (!flagToStopLearning)
            {
                ClearMatrix();
                double D = 0;
                for (int I = 0; I < NumberOfImages; I++)
                {
                    X = _knowledgeMatrix[I];

                    CalculateG();
                    CalculateY();

                    double[] d = CalculateDError(Y, I);
                    double[] e = CalculateEError(Y, d);

                    if (D < d.Max())
                    {
                        D = d.Max();
                    }
                    if (D < Math.Abs(d.Min()))
                    {
                        D = Math.Abs(d.Min());
                    }

                    CalculateW(d);
                    CalculateT(d);
                    CalculateV(e);
                    CalculateQ(e);
                }

                iterator++;
                if (D < DLIMIT)
                {
                    flagToStopLearning = true;
                    MessageBox.Show(iterator.ToString());
                }
            }
        }

        private void ClearMatrix()
        {
            for (int i = 0; i < G.Length; i++)
            {
                G[i] = 0;
            }
            for (int i = 0; i < Y.Length; i++)
            {
                Y[i] = 0;
            }
        }

        private void CalculateG()
        {
            for (int j = 0; j < QSize; j++)
            {
                double sum = 0;
                for (int i = 0; i < _pixelsNumber; i++)
                {
                    sum += X[i] * V[i, j];
                }
                G[j] = ActivationFunction(sum + Q[j]);
            }
        }

        private void CalculateY()
        {
            for (int j = 0; j < NumberOfClasses; j++)
            {
                double sum = 0;
                for (int k = 0; k < QSize; k++)
                {
                    sum += G[k] * W[k, j];
                }
                Y[j] = ActivationFunction(sum + T[j]);
            }
        }

        private void CalculateW(double[] d)
        {
            for (int j = 0; j < QSize; j++)
            {
                for (int k = 0; k < NumberOfClasses; k++)
                {
                    W[j, k] = W[j, k] + ALPHA * Y[k] * (1 - Y[k]) * d[k] * G[j];
                }
            }
        }

        private void CalculateT(double[] d)
        {
            for (int k = 0; k < NumberOfClasses; k++)
            {
                T[k] = T[k] + ALPHA*Y[k]*(1 - Y[k])*d[k];
            }
        }

        private void CalculateV(double[] e)
        {
            for (int i = 0; i < _pixelsNumber; i++)
            {
                for (int j = 0; j < QSize; j++)
                {
                    V[i, j] = V[i, j] + BETA * G[j] * (1 - G[j]) * e[j] * X[i];
                }
            }
        }

        private void CalculateQ(double[] e)
        {
            for (int j = 0; j < QSize; j++)
            {
                Q[j] = Q[j] + BETA * G[j] * (1 - G[j]) * e[j];
            }
        }

        private double[] CalculateDError(double[] y, int res)
        {
            var dErrors = new double[y.Length];
            for (int i = 0; i < y.Length; i++)
            {
                int yy;
                if (i == 0)
                {
                    yy = (res >= 0 && res <= 2) ? 1 : 0;
                }
                else if (i == 1)
                {
                    yy = (res >= 3 && res <= 5) ? 1 : 0;
                }
                else
                {
                    yy = (res >= 6 && res <= 8) ? 1 : 0;
                }
                dErrors[i] = yy - y[i];
            }
            return dErrors;
        }

        private double[] CalculateEError(double[] y, double[] dErrors)
        {
            var eErrors = new double[QSize];
            for (int i = 0; i < QSize; i++)
            {
                for (int j = 0; j < y.Length; j++)
                {
                    eErrors[i] = dErrors[j] * y[j] * (1 - y[j]) * W[i, j];
                }
            }
            return eErrors;
        }

        private double[] GetImageVector(Bitmap image)
        {
            var imageVector = new double[_pixelsNumber];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    imageVector[y * _width + x] = (image.GetPixel(x, y).R > 150) ? 1 : -1;
                }
            }
            return imageVector;
        }

        private Bitmap GetImageFromVector(int[] vector)
        {
            Bitmap bitmap = new Bitmap(_width, _height);
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    bitmap.SetPixel(x, y, vector[y * _width + x] >= 0 ? Color.White : Color.Black);
                }
            }
            return bitmap;
        }

        private double ActivationFunction(double x)
        {
            return 1/(1+Math.Exp(-x));
        }
    }
}
