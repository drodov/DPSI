using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourierTransform
{
    public class ChartValue
    {
        public double X { get; set; }
        public double Y { get; set; }

        public ChartValue(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
