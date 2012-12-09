using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageProcessing
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private Settings _settings;

        public SettingsWindow(Settings settings)
        {
            _settings = settings;
            InitializeComponent();
            FMinTextBox.Text = _settings.Fmin.ToString();
            FMaxTextBox.Text = _settings.Fmax.ToString();
            GMinTextBox.Text = _settings.Gmin.ToString();
            GMaxTextBox.Text = _settings.Gmax.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _settings.Fmin = double.Parse(FMinTextBox.Text);
                _settings.Fmax = double.Parse(FMaxTextBox.Text);
                _settings.Gmin = double.Parse(GMinTextBox.Text);
                _settings.Gmax = double.Parse(GMaxTextBox.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Illegal data!");
            }
            Close();
        }
    }
}
