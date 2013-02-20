using System.Windows;

namespace DPSI_lab_1
{
    /// <summary>
    /// Interaction logic for SetTresholdWindow.xaml
    /// </summary>
    public partial class SetTresholdWindow : Window
    {
        private readonly Treshold _treshold;

        public SetTresholdWindow(Treshold treshold)
        {
            InitializeComponent();
            _treshold = treshold;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (AutoTreshhold.IsChecked == false)
            {
                _treshold.TresholdValue = int.Parse(TreshholdTextBox.Text);
            }
            Close();
        }

        private void AutoTreshhold_Click(object sender, RoutedEventArgs e)
        {
            if (AutoTreshhold.IsChecked == true)
            {
                TreshholdTextBox.IsEnabled = false;
                _treshold.TresholdValue = null;
            }
            else
            {
                TreshholdTextBox.IsEnabled = true;
            }
        }
    }
}
