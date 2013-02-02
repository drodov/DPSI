using System;
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
        private BitmapImage _sourceImage;
        private BitmapImage _greyImage;
        private BitmapImage _binaryImage;
        private BitmapImage _recognizedImage;

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
                try
                {
                    _greyImage = ImageConverter.GetGreyImage(_sourceImage);
                    GreyImage.Source = _greyImage;

                    _binaryImage = ImageConverter.GetBinaryImage(_sourceImage);
                    BinaryImage.Source = _binaryImage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in image processing: " + ex.Message);
                }
            }
        }
    }
}
