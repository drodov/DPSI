using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NumericHepler;

namespace FourierTransformLib
{
    public class FourierTransform
    {
        private readonly int _n;

        public FourierTransform(int n)
        {
            _n = n;
        }

        public Complex[] ForwardTransform(IEnumerable<double> mas)
        {
            var a = new List<Complex>();
            foreach (double real in mas)
            {
                a.Add(new Complex(real, 0));
            }
            return GetArrayInRightOrder(FFT(a.ToArray(), true));
        }

        public double[] BackwardTransform(Complex[] mas)
        {
            mas = GetArrayInRightOrder(FFT(mas, false));
            
            var results = new double[mas.Count()];
            for (int i = 0; i < results.Length; i++ )
            {
                results[i] = mas[i].Real/results.Length;
            }
            
            return results;
        }

        private Complex[] FFT(Complex[] mas, bool isForward)
        {
            int N = mas.Count();

            Complex Wn = CalculateWn(isForward, N);
            int k = mas.Length;
            var y = new Complex[k];
            var W = new Complex(1, 0);
            for (int j = 0; j < k / 2; j++)
            {
                y[j] = mas[j] + mas[j + k / 2];
                y[j + k / 2] = W * (mas[j] - mas[j + k / 2]);
                W = W * Wn;
            }

            var mas11 = new Complex[N / 2];
            var mas22 = new Complex[N / 2];
            for (int i = 0; i < N / 2; i++ )
            {
                mas11[i] = y[i];
                mas22[i] = y[i + N / 2];
            }

            if (N > 2)
            {
                mas11 = FFT(mas11, isForward);
                mas22 = FFT(mas22, isForward);
            }

            var masRes = new Complex[N];
            for (int i = 0; i < N / 2; i ++ )
            {
                masRes[i] = mas11[i];
                masRes[i + N / 2] = mas22[i];
            }

            return masRes;
        }

        private Complex CalculateWn(bool isForward, int n)
        {
            int k = isForward ? 1 : -1;
            return new Complex(Math.Cos(2 * Math.PI / n), k * Math.Sin(2 * Math.PI / n));
        }

        private Complex[] ButterflyMethod(Complex[] a, bool isForward)
        {
            Complex Wn = CalculateWn(isForward, a.Count());
            int k = a.Length;
            var y = new Complex[k];
            var W = new Complex(1, 0);
            for(int j = 0; j < k / 2; j++)
            {
                y[j] = a[j] + a[j + k / 2];
                y[j + k / 2] = W * (a[j] - a[j + k / 2]);
                W = W * Wn;
            }
            return y;
        }

        private Complex[] GetArrayInRightOrder(IEnumerable<Complex> a)
        {
            var arrayInRightOrder = new Complex[a.Count()];
            for (int i = 0; i < a.Count(); i++)
            {
                arrayInRightOrder[NumericHelper.ReverseNumber(i, (int)Math.Log(_n,2))] = a.ElementAt(i);
            }
            return arrayInRightOrder;
        }

        public Complex[] DiscretFourier(Complex[] data, bool isForward)
        {
            int dir = isForward ? 1 : -1;
            var res = new Complex[_n];
            for(int i = 0; i < _n; i++)
            {
                for(int j = 0; j < _n; j++)
                {
                    double r = res[i].Real + data[j].Real * Math.Cos(2 * Math.PI * i * j / _n) - data[j].Imaginary * Math.Sin(2 * Math.PI * i * j / _n) * dir;
                    double im = res[i].Imaginary + data[j].Real * Math.Sin(2 * Math.PI * i * j / _n) * dir + data[j].Imaginary * Math.Cos(2* Math.PI * i * j / _n);
                    res[i] = new Complex(r, im);                
                }
            if(dir == 1)
            {

                res[i] = new Complex(res[i].Real / _n, res[i].Imaginary / _n);   
            }
        }
        return res;
        }
    }
}
