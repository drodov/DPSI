using System;
using System.Drawing;

namespace ImageHelper
{
    public class HSV
    {
        public byte Hue { get; private set; }
        public byte Saturation { get; private set; }
        public byte Value { get; private set; }

        public HSV(byte hue, byte saturation, byte value)
        {
            Hue = hue;
            Saturation = saturation;
            Value = value;
        }

        public HSV(RGB rgb)
        {
            Hue = GetHue(rgb.Red, rgb.Green, rgb.Blue);
            Saturation = GetSaturation(rgb.Red, rgb.Green, rgb.Blue);
            Value = GetValue(rgb.Red, rgb.Green, rgb.Blue);
        }

        public HSV(Color color)
        {
            Hue = GetHue(color.R, color.G, color.B);
            Saturation = GetSaturation(color.R, color.G, color.B);
            Value = GetValue(color.R, color.G, color.B);
        }

        public RGB ToRGB()
        {
            return new RGB(this);
        }

        public Color ToColor()
        {
            var rgb = new RGB(this);
            return Color.FromArgb(rgb.Red, rgb.Green, rgb.Blue);
        }

        public static byte GetHue(byte red, byte green, byte blue)
        {
            double min = Math.Min(Math.Min(red, green), blue);
            double max = Math.Max(Math.Max(red, green), blue);

            if(max == min)
            {
                return 0;
            }

            if (max == red && green >= blue)
            {
                return (byte) (60*(green - blue)/(max - min));
            }

            if (max == red && green < blue)
            {
                return (byte) (60*(green - blue)/(max - min) + 360);
            }

            if (max == green)
            {
                return (byte) (60*(blue - red)/(max - min) + 120);
            }

            if (max == blue)
            {
                return (byte) (60*(red - green)/(max - min) + 240);
            }

            throw new ApplicationException("Can't get hue value from RGB.");
        }

        public static byte GetSaturation(byte red, byte green, byte blue)
        {
            byte min = Math.Min(Math.Min(red, green), blue);
            byte max = Math.Max(Math.Max(red, green), blue);

            if (max == 0)
            {
                return 0;
            }
            
            return (byte) (1 - (min / max));
        }

        public static byte GetValue(byte red, byte green, byte blue)
        {
            return Math.Max(Math.Max(red, green), blue);
        }
    }
}
