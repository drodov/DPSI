using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DPSI_lab_6
{
    /// <summary>
    /// Interaction logic for LearningResultsWindow.xaml
    /// </summary>
    public partial class LearningResultsWindow : Window
    {
        public LearningResultsWindow()
        {
            InitializeComponent();
        }
        public LearningResultsWindow(IEnumerable<LearningResult> learningResults)
        {
            InitializeComponent();

            Image1.Source = learningResults.ElementAt(0).Image;
            Result1.Content = learningResults.ElementAt(0).ClassNumnber;
            Image2.Source = learningResults.ElementAt(1).Image;
            Result2.Content = learningResults.ElementAt(1).ClassNumnber;
            Image3.Source = learningResults.ElementAt(2).Image;
            Result3.Content = learningResults.ElementAt(2).ClassNumnber;
            Image4.Source = learningResults.ElementAt(3).Image;
            Result4.Content = learningResults.ElementAt(3).ClassNumnber;
            Image5.Source = learningResults.ElementAt(4).Image;
            Result5.Content = learningResults.ElementAt(4).ClassNumnber;
            Image6.Source = learningResults.ElementAt(5).Image;
            Result6.Content = learningResults.ElementAt(5).ClassNumnber;
            Image7.Source = learningResults.ElementAt(6).Image;
            Result7.Content = learningResults.ElementAt(6).ClassNumnber;
            Image8.Source = learningResults.ElementAt(7).Image;
            Result8.Content = learningResults.ElementAt(7).ClassNumnber;
            Image9.Source = learningResults.ElementAt(8).Image;
            Result9.Content = learningResults.ElementAt(8).ClassNumnber;
        }
    }
}
