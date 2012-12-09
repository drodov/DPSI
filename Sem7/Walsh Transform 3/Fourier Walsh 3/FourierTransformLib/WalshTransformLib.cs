using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NumericHepler;

namespace WalshTransformLib
{
    public class WalshTransform
    {
        private readonly int _n;
        public int[][] RademacherFunctions { get; private set; }
        public int[][] WalshFunctions { get; private set; }
        public int[][] WalshFunctionsInHadamarOrder { get; private set; }

        public WalshTransform(int n)
        {
            if(Math.Log(n, 2) % 1 != 0)
            {
                throw new ApplicationException("Wrong N!");
            }

            _n = n;

            RademacherFunctions = new int[4][];
            RademacherFunctions[0] = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
            RademacherFunctions[1] = new[] { 1, 1, 1, 1, -1, -1, -1, -1 };
            RademacherFunctions[2] = new[] { 1, 1, -1, -1, 1, 1, -1, -1 };
            RademacherFunctions[3] = new[] { 1, -1, 1, -1, 1, -1, 1, -1 };

            WalshFunctions = new int[n][];
            WalshFunctions[0] = new[]{ 1, 1, 1, 1, 1, 1, 1, 1};
            var nn = new[] {0, 0, 0, 0};
            var rr = new int[3];
            for (int i = 1; i < n; i++ )
            {
                string bin = NumericHelper.IntToBinaryString(i, 3);
                nn[3] = Convert.ToInt32(bin[0].ToString(), 10);
                nn[2] = Convert.ToInt32(bin[1].ToString(), 10);
                nn[1] = Convert.ToInt32(bin[2].ToString(), 10);

                for(int j = nn.Length - 1; j > 0; j--)
                {
                    rr[nn.Length - 1 - j] = XOR(nn[j], nn[j - 1]);
                }

                WalshFunctions[i] = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
                for(int j = 0; j < rr.Length; j++)
                {
                    if(rr[j] == 1)
                    {
                        WalshFunctions[i] = NumericHelper.MultiplyArrays(WalshFunctions[i], RademacherFunctions[j + 1]);
                    }
                }
            }

            SetWalshFunctionsInHadamarOrder();
        }

        private void SetWalshFunctionsInHadamarOrder()
        {
            WalshFunctionsInHadamarOrder = new[]
                {
                    WalshFunctions[0], WalshFunctions[4], WalshFunctions[6], WalshFunctions[2], WalshFunctions[3],
                    WalshFunctions[7], WalshFunctions[5], WalshFunctions[1]
                };
        }

        public Complex[] ForwardTransform(IEnumerable<double> mas)
        {
            var a = new List<Complex>();
            foreach (double real in mas)
            {
                a.Add(new Complex(real, 0));
            }

            Complex[] masTrans =  FWT(a.ToArray(), true);
            /*var results = new Complex[mas.Count()];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = new Complex(masTrans[i].Real / results.Length, masTrans[i].Imaginary / results.Length);
            }*/

            return masTrans;
        }

        public double[] BackwardTransform(Complex[] mas)
        {
            mas = FWT(mas, false);
            var results = new double[mas.Count()];
            for (int i = 0; i < results.Length; i++ )
            {
                results[i] = mas[i].Real/results.Length;
            }
            
            return results;
        }

        private Complex[] FWT(Complex[] mas, bool isForward)
        {
            int N = mas.Length;
            int k = N;

            var y = new Complex[k];
            for (int j = 0; j < k / 2; j++)
            {
                y[j] = mas[j] + mas[j + k / 2];
                y[j + k / 2] = mas[j] - mas[j + k / 2];
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
                mas11 = FWT(mas11, isForward);
                mas22 = FWT(mas22, isForward);
            }

            var masRes = new Complex[N];
            for (int i = 0; i < N / 2; i ++ )
            {
                masRes[i] = mas11[i];
                masRes[i + N / 2] = mas22[i];
            }

            return masRes;
        }

        private int XOR(int a, int b)
        {
            if(a != b)
            {
                return 1;
            }

            return 0;
        }
    }
}
