using System.Windows;

namespace DPSI_lab_1
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly Settings _settings;

        public SettingsWindow(Settings settings)
        {
            InitializeComponent();
            _settings = settings;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (AutoTreshhold.IsChecked == false)
            {
                _settings.TresholdValue = int.Parse(TreshholdTextBox.Text);
            }
            _settings.K = int.Parse(KTextBox.Text);
            _settings.Inversion = (bool)InversionCheckBox.IsChecked;
            Close();
        }

        private void AutoTreshhold_Click(object sender, RoutedEventArgs e)
        {
            if (AutoTreshhold.IsChecked == true)
            {
                TreshholdTextBox.IsEnabled = false;
                _settings.TresholdValue = null;
            }
            else
            {
                TreshholdTextBox.IsEnabled = true;
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            KTextBox.Text = _settings.K.ToString();
            InversionCheckBox.IsChecked = _settings.Inversion;
            if (_settings.TresholdValue != null)
            {
                AutoTreshhold.IsChecked = false;
                TreshholdTextBox.IsEnabled = true;
                TreshholdTextBox.Text = _settings.TresholdValue.ToString();
            }
            else
            {
                AutoTreshhold.IsChecked = true;
                TreshholdTextBox.IsEnabled = false;
            }
        }
    }
}
