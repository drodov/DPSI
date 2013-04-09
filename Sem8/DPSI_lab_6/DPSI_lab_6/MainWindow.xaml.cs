using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Color = System.Drawing.Color;

using BitmapHelper;

namespace DPSI_lab_6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double BETA = 1;
        private const int ATTEMPT_LIMIT = 1000;
        private const int NumberOfClasses = 3;
        private const int NumberOfImages = 9;
        private BitmapImage _sourceImage;
        private BitmapImage _noisedImage;
        private int _height;
        private int _width;
        private int _pixelsNumber;
        private int _noiseValue;
        private int[] _frequency = new int[NumberOfClasses];
        private double[] _distance = new double[NumberOfClasses];
        private readonly BitmapImage[] _knowledgeImages = new BitmapImage[NumberOfImages];
        private readonly double[][] _knowledgeMatrix = new double[NumberOfImages][];
        private double[] X;
        private double[,] W;
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

            _width = _knowledgeImages[0].PixelWidth;
            _height = _knowledgeImages[0].PixelHeight;
            _pixelsNumber = _width * _height;
            
            _knowledgeImages[1] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "C")));
            _knowledgeImages[2] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "K")));
            _knowledgeImages[3] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "A2")));
            _knowledgeImages[4] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "C2")));
            _knowledgeImages[5] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "K2")));
            _knowledgeImages[6] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "A3")));
            _knowledgeImages[7] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "C3")));
            _knowledgeImages[8] = new BitmapImage(new Uri(string.Format("{0}{1}.png", baseUri, "K3")));
            
            for (int i = 0; i < NumberOfImages; i++)
            {
                _knowledgeMatrix[i] = GetImageVector(BitmapConverter.BitmapImage2Bitmap(_knowledgeImages[i]));
            }

            W = new double[_pixelsNumber, NumberOfClasses];
            for (int i = 0; i < _pixelsNumber; i++)
            {
                for (int j = 0; j < NumberOfClasses; j++)
                {
                    W[i, j] = 1;
                }
            }

            InitFrequency();

            Y = new double[NumberOfClasses];

            StartLearning();

            var learningResults = new List<LearningResult>();
            for(int i = 0; i < _knowledgeImages.Length; i++)
            {
               learningResults.Add(new LearningResult
                   {
                       Image = _knowledgeImages[i],
                       ClassNumnber = RecognizeClass(_knowledgeImages[i])
                   }); 
            }

            var learningResultsWindow = new LearningResultsWindow(learningResults);
            learningResultsWindow.Show();
        }

        private void InitFrequency()
        {
            for (int i = 0; i < _frequency.Length; i++)
            {
                _frequency[i] = 1;
            }
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

            CalculateY();

            Result1.Content = Y[0];
            Result4.Content = Y[1];
            Result7.Content = Y[2];

            for (int i = 0; i < Y.Length; i++)
            {
                if (Y[i] == Y.Max())
                {
                    MessageBox.Show("Class: " + (i + 1).ToString());
                }
            }
        }

        private int RecognizeClass(BitmapImage image)
        {
            X = GetImageVector(BitmapConverter.BitmapImage2Bitmap(image));

            CalculateY();

            for (int i = 0; i < Y.Length; i++)
            {
                if (Y[i] == Y.Max())
                {
                    return i + 1;
                }
            }

            return 0;
        }

        private void StartLearning()
        {
            bool flagToStopLearning = false;
            int iterator = 0;

            while (!flagToStopLearning)
            {
                for (int I = 0; I < NumberOfImages; I++)
                {
                    X = _knowledgeMatrix[I];
                    /*
                    CalculateY();
                    CalculateDistance(I);
                    int idxToEdit = GetIndexToEdit();
                    _frequency[idxToEdit]++;*/

                    CalculateY();
                    int idxToEdit = GetIndexToEdit();
                    CalculateDistance();

                    CalculateW(idxToEdit);
                }

                iterator++;
                if (iterator == ATTEMPT_LIMIT)
                {
                    flagToStopLearning = true;
                    MessageBox.Show(iterator.ToString());
                }
            }
        }

        private void CalculateY()
        {
            for (int j = 0; j < NumberOfClasses; j++)
            {
                double sum = 0;
                for (int i = 0; i < _pixelsNumber; i++)
                {
                    sum += W[i, j] * X[i];
                }
                Y[j] = sum;
            }
        }

        private void CalculateDistance()
        {
            for (int i = 0; i < NumberOfClasses; i++)
            {
                double sum = 0;
                for (int j = 0; j < _pixelsNumber; j++)
                {
                    sum += Math.Pow(X[i] - W[j, i], 2);
                }
                _distance[i] = Math.Sqrt(sum)/**_frequency[i]*/;
            }
        }

        private void CalculateDistance(int classIdx)
        {
                double sum = 0;
                for (int i = 0; i < _pixelsNumber; i++)
                {
                    sum += Math.Pow(X[i] - W[i, classIdx], 2);
                }
                _distance[classIdx] = Math.Sqrt(sum)/**_frequency[i]*/;
        }

        private int GetIndexToEdit()
        {/*
            double minDistance = _distance.Min();
            return
                _distance.Select((arg, idx) => new {Value = arg, Index = idx}).First(obj => obj.Value == minDistance).Index;*/
            double maxY = Y.Max();
            return
                Y.Select((arg, idx) => new { Value = arg, Index = idx }).First(obj => obj.Value == maxY).Index;
        }

        private void CalculateW(int idxToCorrect)
        {
            for (int i = 0; i < _pixelsNumber; i++)
            {
                W[i, idxToCorrect] = W[i, idxToCorrect] + BETA * (X[i] - W[i, idxToCorrect]);
            }
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
    }
}
