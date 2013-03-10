using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BitmapHelper;
using Microsoft.Win32;
using Color = System.Drawing.Color;

namespace HopfieldNetwork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage _sourceImage;
        private BitmapImage _noisedImage;
        private int _noiseValue;
        private int _pixelsNumber;
        private int _width;
        private int _height;
        private readonly List<BitmapImage> _standardImages = new List<BitmapImage>();
        private readonly List<int[]> _standardImageVectors = new List<int[]>();
        private int[,] _knowledgeMatrix;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = @"C:\Users\Dmitry\Pictures",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() != null)
            {
                try
                {
                    var image = new BitmapImage(new Uri(openFileDialog.FileName));
                    if(_height == 0)
                    {
                        _height = image.PixelHeight;
                        _width = image.PixelWidth;
                        _pixelsNumber = _width*_height;
                    }
                    _standardImages.Add(image);
                    _standardImageVectors.Add(GetImageVector(BitmapConverter.BitmapImage2Bitmap(image)));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = @"C:\Users\Dmitry\Pictures",
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

        private void NoiseButton_Click(object sender, RoutedEventArgs e)
        {
            if(!int.TryParse(NoiseValueTextBox.Text, out _noiseValue))
            {
                MessageBox.Show("Bad noise value!");
                return;
            }
            Bitmap bitmap = BitmapConverter.BitmapImage2Bitmap(_sourceImage);
            var random = new Random();
            var pointsHashSet = new HashSet<Point>();
            int pixelsToInvert = _pixelsNumber * _noiseValue / 100;
            for(int i = 0; i < pixelsToInvert; i++)
            {
                bool wasInverted = false;
                while(!wasInverted)
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

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            NoiseValueTextBox.Text = (sender as Slider).Value.ToString();
        }

        private void RecognizeButton_Click(object sender, RoutedEventArgs e)
        {
            _knowledgeMatrix = new int[_pixelsNumber, _pixelsNumber];
            foreach (var stdVector in _standardImageVectors)
            {
                for(int i = 0; i < _pixelsNumber; i++)
                {
                    for(int j = 0; j < _pixelsNumber; j++)
                    {
                        _knowledgeMatrix[j, i] += (i == j) ? 0 : stdVector[j]*stdVector[i];
                    }
                }
            }
            foreach (var stdVector in _standardImageVectors)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        _knowledgeMatrix[j, i] += (i == j) ? 0 : stdVector[j] * stdVector[i];
                    }
                }
            }

            Bitmap sourceBitmap = BitmapConverter.BitmapImage2Bitmap(_noisedImage);
            int[] sourceImageVector = GetImageVector(sourceBitmap);
            int counter = 0;
            while(true)
            {
                int recognizedImageIdx = IsImageRecognized(sourceImageVector);
                if (recognizedImageIdx != -1)
                {
                    ResultImage.Source = _standardImages[recognizedImageIdx];
                    return;
                }
                sourceImageVector = MultiplyMatrixAndColumn(_knowledgeMatrix, sourceImageVector);
                counter++;
                if(counter == 100)
                {
                    ResultImage.Source = null;
                    MessageBox.Show("Couldn't recognize :(");
                    return;
                }
            }
        }

        private int IsImageRecognized(int[] imageVector)
        {
            for(int idx = 0; idx < _standardImageVectors.Count; idx++)
            {
                bool flagMatch = true;
                for(int i = 0; i < _standardImageVectors[idx].Length; i++)
                {
                    if(imageVector[i] != _standardImageVectors[idx][i])
                    {
                        flagMatch = false;
                    }
                }
                if(flagMatch)
                {
                    return idx;
                }
            }
            return -1;
        }

        private int[] MultiplyMatrixAndColumn(int[,] matrix, int[] column)
        {
            var result = new int[_pixelsNumber];
            for(int i = 0; i < _pixelsNumber; i++)
            {
                for (int j = 0; j < _pixelsNumber; j++)
                {
                    result[i] += matrix[j, i]*column[j];
                }
                result[i] = (result[i] >= 0) ? 1 : -1;
            }
            return result;
        }

        private int[] GetImageVector(Bitmap image)
        {
            var imageVector = new int[_pixelsNumber];
            for(int y = 0 ; y < _height; y++)
            {
                for(int x = 0; x < _width; x++)
                {
                    imageVector[y*_width + x] = (image.GetPixel(x, y).R > 150) ? 1 : -1;
                }
            }
            return imageVector;
        }
    }
}
