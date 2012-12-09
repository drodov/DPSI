using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Numerics;
using System.Collections.ObjectModel;
using System.Windows.Controls.DataVisualization.Charting;

namespace FourierTransform
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int N = 16;
        private const double _d = 2 * Math.PI / (N);
        private List<double> _args = new List<double> { 0 };
        private List<double> _myFuncResults = new List<double>();
        private List<double> _yFuncResults = new List<double>();
        private List<double> _zFuncResults = new List<double>();
        private double[] _convolutionResults;
        private double[] _correlationResults;
        private Complex[] _yForwardFourier;
        private Complex[] _zForwardFourier;

        private FourierTransformLib.FourierTransform _FourierTransform;

        public ObservableCollection<ChartValue> BaseChartValues { get; set; }
        public ObservableCollection<ChartValue> YBaseChartValues { get; set; }
        public ObservableCollection<ChartValue> ZBaseChartValues { get; set; }

        public ObservableCollection<ChartValue> ConvolutionChartValues { get; set; }
        public ObservableCollection<ChartValue> CorrelationChartValues { get; set; }
        
        public ObservableCollection<ObservableCollection<ChartValue>> ChartSource { get; set; }
        public ObservableCollection<ObservableCollection<ChartValue>> ConvolutionChartSource { get; set; }
        public ObservableCollection<ObservableCollection<ChartValue>> CorrelationChartSource { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private double MyFunction(double x)
        {
            return Math.Sin(x) + Math.Cos(4 * x);
        }
        
        private double YFunction(double x)
        {
            return Math.Sin(x);
        }
        
        private double ZFunction(double x)
        {
            return Math.Cos(4 * x);
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < N - 1; i++)
            {
                _args.Add(_args[i] + _d);
            }

            BaseChartValues = new ObservableCollection<ChartValue>();
            YBaseChartValues = new ObservableCollection<ChartValue>();
            ZBaseChartValues = new ObservableCollection<ChartValue>();

            int j = 0;
            foreach (var arg in _args)
            {
                //_myFuncResults.Add(MyFunction(arg));
                //BaseChartValues.Add(new ChartValue(arg, _myFuncResults[j]));

                _yFuncResults.Add(YFunction(arg));
                YBaseChartValues.Add(new ChartValue(arg, _yFuncResults[j]));

                _zFuncResults.Add(ZFunction(arg));
                ZBaseChartValues.Add(new ChartValue(arg, _zFuncResults[j]));
                
                j++;
            }

            ChartSource = new ObservableCollection<ObservableCollection<ChartValue>> {BaseChartValues, YBaseChartValues, ZBaseChartValues};
            BaseChart.DataContext = ChartSource;

            ConvolutionChartValues = new ObservableCollection<ChartValue>();
            ConvolutionChartSource = new ObservableCollection<ObservableCollection<ChartValue>>() { ConvolutionChartValues };
            ConvolutionChart.DataContext = ConvolutionChartSource;

            CorrelationChartValues = new ObservableCollection<ChartValue>();
            CorrelationChartSource = new ObservableCollection<ObservableCollection<ChartValue>>() { CorrelationChartValues };
            CorrelationChart.DataContext = CorrelationChartSource;

            _FourierTransform = new FourierTransformLib.FourierTransform(N);
        }

        private void CalcConvolutionButton_Click(object sender, RoutedEventArgs e)
        {
            ConvolutionTabItem.IsEnabled = true;
            ConvolutionTabItem.IsSelected = true;

            _yForwardFourier = _FourierTransform.ForwardTransform(_yFuncResults);
            _zForwardFourier = _FourierTransform.ForwardTransform(_zFuncResults);

            var temp = new Complex[N];
            for(int i = 0; i < N; i++)
            {
                temp[i] = _yForwardFourier[i] * _zForwardFourier[i];
            }

            _convolutionResults = _FourierTransform.BackwardTransform(temp);
            int j = 0;
            foreach(var convolRes in _convolutionResults)
            {
                ConvolutionChartValues.Add(new ChartValue(_args[j], convolRes));
                j++;
            }
        }

        private void CalcCorrelationButton_Click(object sender, RoutedEventArgs e)
        {
            CorrelationTabItem.IsEnabled = true;
            CorrelationTabItem.IsSelected = true;

            var _yForwardFourierConjugated = new Complex[N];
            for(int i = 0; i < N; i++)
            {
                _yForwardFourierConjugated[i] = Complex.Conjugate(_yForwardFourier[i]);
            }

            var temp = new Complex[N];
            for (int i = 0; i < N; i++)
            {
                temp[i] = _yForwardFourierConjugated[i] * _zForwardFourier[i];
            }

            _correlationResults = _FourierTransform.BackwardTransform(temp);
            int j = 0;
            foreach (var correlRes in _correlationResults)
            {
                CorrelationChartValues.Add(new ChartValue(_args[j], correlRes));
                j++;
            }
        }
    }
}
