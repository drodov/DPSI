using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageHelper;
using Microsoft.Win32;

namespace DPSI_lab_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ImageConverter _imageConverter;
        private BitmapImage _sourceImage;
        private BitmapImage _greyImage;
        private BitmapImage _binaryImage;
        private BitmapImage _recognizedImage;

        private readonly Settings _settings = new Settings
            {
                Inversion = true,
                K = 2,
                TresholdValue = 200
            };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFileClick(object sender, RoutedEventArgs e)
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
                    SourceImage.Source = _sourceImage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
                RunImageProcessing();
            }
        }

        private void SetTreshold(object sender, RoutedEventArgs e)
        {
            var setTresholdWindow = new SettingsWindow(_settings);
            setTresholdWindow.ShowDialog();
            RunImageProcessing();
        }

        private void RunImageProcessing()
        {
            try
            {
                _imageConverter = new ImageConverter(_sourceImage, _settings.TresholdValue, _settings.K, _settings.Inversion);

                _greyImage = _imageConverter.GreyImage;
                GreyImage.Source = _greyImage;

                _binaryImage = _imageConverter.BinaryImage;
                BinaryImage.Source = _binaryImage;

                _recognizedImage = _imageConverter.RecognizedImage;
                RecognizedImage.Source = _recognizedImage;
                MessageBox.Show("OK!");
                RecognizedImage.Source = _imageConverter.ClusteredImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in image processing: " + ex.Message);
            }
        }
    }
}
