using System;

namespace ImageHelper
{
    public class RGB
    {
        public byte Red { get; private set; }
        public byte Green { get; private set; }
        public byte Blue { get; private set; }

        public RGB(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public RGB(HSV hsv)
        {
            byte Hi = (byte) (hsv.Hue/60);
            byte Vmin = (byte) ((100 - hsv.Saturation)*hsv.Value/100);
            byte a = (byte) ((hsv.Value - Vmin)*(hsv.Hue%60)/60);
            byte Vinc =  (byte) (Vmin + a);
            byte Vdec =  (byte) (hsv.Value - a);

            switch (Hi)
            {
                case 0:
                    Red = hsv.Value;
                    Green = Vinc;
                    Blue = Vmin;
                    break;
                case 1:
                    Red = Vdec;
                    Green = hsv.Value;
                    Blue = Vmin;
                    break;
                case 2:
                    Red = Vmin;
                    Green = hsv.Value;
                    Blue = Vinc;
                    break;
                case 3:
                    Red = Vmin;
                    Green = Vdec;
                    Blue = hsv.Value;
                    break;
                case 4:
                    Red = Vinc;
                    Green = Vmin;
                    Blue = hsv.Value;
                    break;
                case 5:
                    Red = hsv.Value;
                    Green = Vmin;
                    Blue = Vdec;
                    break;
                default:
                    throw new ApplicationException("Can'tconvert from HSV to RGB");
            }
        }

        public HSV ToHSV()
        {
            return new HSV(this);
        }
    }
}
