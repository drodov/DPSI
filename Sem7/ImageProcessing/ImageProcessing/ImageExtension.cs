using System;
using System.Drawing;

namespace ImageProcessing
{
    public static class ImageExtension
    {
        public static int Max(this Color color)
        {
            return (color.R > color.B)
                       ? (color.R > color.G) ? color.R : color.G
                       : (color.B > color.G) ? color.B : color.G;
        }

        public static int Min(this Color color)
        {
            return (color.R > color.B)
                       ? (color.B > color.G) ? color.G : color.B
                       : (color.R > color.G) ? color.G : color.R;
        }
    }
}
