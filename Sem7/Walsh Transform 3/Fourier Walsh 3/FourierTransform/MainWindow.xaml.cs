using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private const int N = 8;
        private const double _d = 2 * Math.PI / (N);
        private const int NumFunc = 3;
        private List<double> _args = new List<double> { 0 };
        private List<double> _myFuncResults = new List<double>();
        private List<double> _rademacherFuncResults = new List<double>();
        private List<double> _hadamarFuncResults = new List<double>();
        private double[] _BWTRResults;
        private double[] _BWTHResults;
        private double[] _correlationResults;
        private Complex[] _FWTRResults;
        private Complex[] _FWTHResults;

        private WalshTransformLib.WalshTransform _WalshTransform;

        public ObservableCollection<ObservableCollection<ChartValue>> ChartSource { get; set; }
        public ObservableCollection<ChartValue> BaseChartValues { get; set; }
        public ObservableCollection<ChartValue> RBaseChartValues { get; set; }
        public ObservableCollection<ChartValue> HBaseChartValues { get; set; }

        public ObservableCollection<ObservableCollection<ChartValue>> FWTChartSource { get; set; }
        public ObservableCollection<ChartValue> FWTRChartValues { get; set; }
        public ObservableCollection<ChartValue> FWTHChartValues { get; set; }
        
        public ObservableCollection<ObservableCollection<ChartValue>> BWTChartSource { get; set; }
        public ObservableCollection<ChartValue> BWTRChartValues { get; set; }
        public ObservableCollection<ChartValue> BWTHChartValues { get; set; }

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
            WalshTransformInit();

            for (int i = 0; i < N - 1; i++)
            {
                _args.Add(_args[i] + _d);
            }

            BaseChartValues = new ObservableCollection<ChartValue>();
            RBaseChartValues = new ObservableCollection<ChartValue>();
            HBaseChartValues = new ObservableCollection<ChartValue>();

            int j = 0;
            foreach (var arg in _args)
            {
                _myFuncResults.Add(MyFunction(arg));
                BaseChartValues.Add(new ChartValue(arg, _myFuncResults[j]));

                _rademacherFuncResults.Add(MyFunction(arg) * _WalshTransform.WalshFunctions[NumFunc][j]);
                RBaseChartValues.Add(new ChartValue(arg, _rademacherFuncResults[j]));

                _hadamarFuncResults.Add(MyFunction(arg) * _WalshTransform.WalshFunctionsInHadamarOrder[NumFunc][j]);
                HBaseChartValues.Add(new ChartValue(arg, _hadamarFuncResults[j]));
                
                j++;
            }

            ChartSource = new ObservableCollection<ObservableCollection<ChartValue>> {BaseChartValues, RBaseChartValues, HBaseChartValues};
            BaseChart.DataContext = ChartSource;

            FWTRChartValues = new ObservableCollection<ChartValue>();
            FWTHChartValues = new ObservableCollection<ChartValue>();
            FWTChartSource = new ObservableCollection<ObservableCollection<ChartValue>>() { FWTRChartValues, FWTHChartValues };
            FWTChart.DataContext = FWTChartSource;

            BWTRChartValues = new ObservableCollection<ChartValue>();
            BWTHChartValues = new ObservableCollection<ChartValue>();
            BWTChartSource = new ObservableCollection<ObservableCollection<ChartValue>>() { BWTRChartValues, BWTHChartValues };
            BWTChart.DataContext = BWTChartSource;
        }

        private void WalshTransformInit()
        {
            _WalshTransform = new WalshTransformLib.WalshTransform(N);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("I.");
            foreach (var WalshFunc in _WalshTransform.WalshFunctions)
            {
                foreach (var v in WalshFunc)
                {
                    stringBuilder.Append(string.Format(" {0}", v > 0 ? '+' : '-'));
                }
                stringBuilder.AppendLine();
            }
            stringBuilder.AppendLine("II.");
            foreach (var WalshFunc in _WalshTransform.WalshFunctionsInHadamarOrder)
            {
                foreach (var v in WalshFunc)
                {
                    stringBuilder.Append(string.Format(" {0}", v > 0 ? '+' : '-'));
                }
                stringBuilder.AppendLine();
            }
            InfoTextBox.Text = stringBuilder.ToString();
        }

        private void CalcFWTButton_Click(object sender, RoutedEventArgs e)
        {
            FWTTabItem.IsEnabled = true;
            FWTTabItem.IsSelected = true;

            _FWTRResults = _WalshTransform.ForwardTransform(_rademacherFuncResults);
            _FWTHResults = _WalshTransform.ForwardTransform(_hadamarFuncResults);

            FWTRChartValues.Clear();
            FWTHChartValues.Clear();
            for(int i = 0; i < _FWTRResults.Length; i++)
            {
                FWTRChartValues.Add(new ChartValue(_args[i], _FWTRResults[i].Magnitude));
                FWTHChartValues.Add(new ChartValue(_args[i], _FWTHResults[i].Magnitude));
            }
        }

        private void CalcBWTButton_Click(object sender, RoutedEventArgs e)
        {
            BWTTabItem.IsEnabled = true;
            BWTTabItem.IsSelected = true;

            _BWTRResults = _WalshTransform.BackwardTransform(_FWTRResults);
            _BWTHResults = _WalshTransform.BackwardTransform(_FWTHResults);

            BWTRChartValues.Clear();
            BWTHChartValues.Clear();
            for (int i = 0; i < _BWTRResults.Length; i++)
            {
                BWTRChartValues.Add(new ChartValue(_args[i], _BWTRResults[i]));
                BWTHChartValues.Add(new ChartValue(_args[i], _BWTHResults[i]));
            }
        }
    }
}
