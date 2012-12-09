using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Numerics;
using System.Collections.ObjectModel;

namespace FourierTransform
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int N = 16;
        private const double _d = 2 * Math.PI / (N);
        private List<double> _a = new List<double>();
        private List<double> _args = new List<double> {0};
        private Complex[] _fourierTransformResults;
        private FourierTransformLib.FourierTransform _FourierTransform;
        private Complex[] _discrRes;

        public ObservableCollection<ChartValue> BaseChartValues { get; set; }
        public ObservableCollection<ChartValue> DirectFourierChartValues { get; set; }
        public ObservableCollection<ChartValue> ReverseFourierChartValues { get; set; }
        public ObservableCollection<ChartValue> DiscrDirectFourierChartValues { get; set; }
        public ObservableCollection<ChartValue> DiscrReverseFourierChartValues { get; set; }
        public ObservableCollection<ObservableCollection<ChartValue>> ChartSource { get; set; }
        public ObservableCollection<ObservableCollection<ChartValue>> TransformedChartSource { get; set; }
        public ObservableCollection<ObservableCollection<ChartValue>> ReverseTransformedChartSource { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private double MyFunction(double x)
        {
            return Math.Sin(x) + Math.Cos(4 * x);
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < N - 1; i++)
            {
                _args.Add(_args[i] + _d);
            }

            BaseChartValues = new ObservableCollection<ChartValue>();
            foreach (var arg in _args)
            {
                double res = MyFunction(arg);
                _a.Add(res);
                BaseChartValues.Add(new ChartValue(arg, res));
            }
            ChartSource = new ObservableCollection<ObservableCollection<ChartValue>> {BaseChartValues};
            Chart.DataContext = ChartSource;

            TransformedChartSource = new ObservableCollection<ObservableCollection<ChartValue>>();
            DirectFourierChartValues = new ObservableCollection<ChartValue>();
            TransformedChartSource.Add(DirectFourierChartValues);
            DiscrDirectFourierChartValues = new ObservableCollection<ChartValue>();
            TransformedChartSource.Add(DiscrDirectFourierChartValues);
            TransformedChart.DataContext = TransformedChartSource;

            ReverseTransformedChartSource = new ObservableCollection<ObservableCollection<ChartValue>>();
            ReverseFourierChartValues = new ObservableCollection<ChartValue>();
            ReverseTransformedChartSource.Add(ReverseFourierChartValues);
            DiscrReverseFourierChartValues = new ObservableCollection<ChartValue>();
            ReverseTransformedChartSource.Add(DiscrReverseFourierChartValues);
            ReverseTransformedChart.DataContext = ReverseTransformedChartSource;

            _FourierTransform = new FourierTransformLib.FourierTransform(N);
        }

        private void TransformButton_Click(object sender, RoutedEventArgs e)
        {
            _fourierTransformResults = _FourierTransform.ForwardTransform(_a);

            Complex[] cc = new Complex[N];
            for(int i = 0; i < N; i++)
            {
                cc[i] = new Complex(_a[i], 0);
            }
            _discrRes = _FourierTransform.DiscretFourier(cc, true);

            DirectFourierChartValues.Clear();
            for (int i = 0; i < _fourierTransformResults.Count(); i++)
            {
                //var complexNumberInAmplitudePhaseForm = NumericHelper.GetComplexNumberInAmplitudePhaseForm(_fourierTransformResults.ElementAt(i));
                //DirectFourierChartValues.Add(new ChartValue(complexNumberInAmplitudePhaseForm.Phase, complexNumberInAmplitudePhaseForm.Amplitude));
                DirectFourierChartValues.Add(new ChartValue(_fourierTransformResults.ElementAt(i).Phase, _fourierTransformResults.ElementAt(i).Magnitude));
                DiscrDirectFourierChartValues.Add(new ChartValue(_discrRes.ElementAt(i).Phase, _discrRes.ElementAt(i).Magnitude));
            }

            TransformTabItem.IsEnabled = true;
            TransformTabItem.IsSelected = true;
            ReverseTransformTabItem.IsEnabled = false;
        }

        private void ReverseTransformButton_Click(object sender, RoutedEventArgs e)
        {
            double[] res = _FourierTransform.BackwardTransform(_fourierTransformResults);

            Complex[] resD = _FourierTransform.DiscretFourier(_discrRes, false);

            int j = 0;
            ReverseFourierChartValues.Clear();
            foreach (var r in res)
            {
                ReverseFourierChartValues.Add(new ChartValue(BaseChartValues[j].X, r));
                DiscrReverseFourierChartValues.Add(new ChartValue(BaseChartValues[j].X, resD[j].Real));
                j++;
            }

            ReverseTransformTabItem.IsEnabled = true;
            ReverseTransformTabItem.IsSelected = true;
        }
    }
}
