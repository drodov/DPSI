using System;
using System.Drawing;

namespace ImageProcessing
{
    public class HSV
    {
        private int _Hue = 0;
        private double _Saturation = 0;
        private double _Value = 0;

        public HSV(Color rgb)
        {
            ColorToHSV(rgb);
        }

        public int Hue
        {
            get { return _Hue; }
            set { _Hue = value; }
        }

        public double Saturation
        {
            get { return _Saturation; }
            set { _Saturation = value; }
        }

        public double Value
        {
            get { return _Value; }
            set { _Value = value; }
        }


        private void ColorToHSV(Color rgb)
        {
            var max = rgb.Max();
            var min = rgb.Min();
            //Hue
            if (min == max)
            {
                _Hue = 0;
            }
            else if (rgb.R == max && rgb.G >= rgb.B)
            {
                _Hue = (int)Math.Abs(60 * ((double)(rgb.G - rgb.B) / (max - min)));
            }
            else if (rgb.R == max && rgb.G < rgb.B)
            {
                _Hue = (int)Math.Abs(60 * ((double)(rgb.G - rgb.B) / (max - min)) + 360);
            }
            else if (rgb.G == max)
            {
                _Hue = (int)Math.Abs(60 * ((double)(rgb.B - rgb.R) / (max - min)) + 120);
            }
            else if (rgb.B == max)
            {
                _Hue = (int)Math.Abs(60 * ((double)(rgb.R - rgb.G) / (max - min)) + 240);
            }

            //Saturation
            if (max == 0)
            {
                _Saturation = 0;
            }
            else
            {
                _Saturation = 1 - (double)min * 1 / max;
            }

            //Value
            _Value = (double)max / 256;
        }

        public Color HSVToColor()
        {
            var Vmin = (1 - _Saturation) * _Value;
            var a = ((_Value - Vmin) * (_Hue % 60)) / 60;
            var Vinc = Vmin + a;
            var Vdec = _Value - a;
            var Hi = (int)Math.Abs((double)_Hue / 60);
            double R = 0;
            double B = 0;
            double G = 0;

            switch (Hi)
            {
                case 0:
                    R = _Value;
                    G = Vinc;
                    B = Vmin;
                    break;
                case 1:
                    R = Vdec;
                    G = _Value;
                    B = Vmin;
                    break;
                case 2:
                    R = Vmin;
                    G = _Value;
                    B = Vinc;
                    break;
                case 3:
                    R = Vmin;
                    G = Vdec;
                    B = _Value;
                    break;
                case 4:
                    R = Vinc;
                    G = Vmin;
                    B = _Value;
                    break;
                case 5:
                    R = _Value;
                    G = Vmin;
                    B = Vdec;
                    break;
            }
            return Color.FromArgb((byte)(R * 255), (byte)(G * 255), (byte)(B * 255));
        }

        public static explicit operator Color(HSV param)
        {
            return param.HSVToColor();
        }
    }
}
