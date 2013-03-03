using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ImageHelper
{
    public class ImageConverter
    {
        private readonly int K;
        private readonly bool IsInverted;
        private int _counter = 0;
        private readonly int _height;
        private readonly int _width;
        private readonly int _pixelsNumber;
        private int _treshold;
        private readonly int[,] _labels;
        private readonly int[,] _borders;
        private readonly bool _needCalculateTreshold = true;
        private HashSet<int> _colorsHashSet;

        private Dictionary<int, int> _square;
        private Dictionary<int, double> _xCenter;
        private Dictionary<int, double> _yCenter;
        private Dictionary<int, int> _perimeter = new Dictionary<int, int>();
        private readonly Dictionary<int, double> _compactness = new Dictionary<int, double>();
        private readonly Dictionary<int, double> _elongation = new Dictionary<int, double>();
        private readonly Dictionary<int, double> _m02 = new Dictionary<int, double>();
        private readonly Dictionary<int, double> _m11 = new Dictionary<int, double>();
        private readonly Dictionary<int, double> _m20 = new Dictionary<int, double>();
        
        private readonly List<Vector> _vectors = new List<Vector>();
        private readonly Dictionary<int, int> _vectorForObject = new Dictionary<int, int>(); 

        public BitmapImage SourceImage { get; private set; }
        public BitmapImage GreyImage { get; private set; }
        public BitmapImage BinaryImage { get; private set; }
        public BitmapImage RecognizedImage { get; private set; }
        public BitmapImage ClusteredImage
        {
            get
            {
                foreach (var color in _colorsHashSet)
                {
                    _vectorForObject.Add(color, 0);
                }

                _square = GetSquare();
                CalculateMassCenters();
                CalculatePerimeter();
                CalculateCompactness();
                CalculateElongation();

                return DefineObjects();
            }
        }

        public ImageConverter(BitmapImage sourceImage, int? treshold, int k, bool isInverted)
        {
            K = k;
            IsInverted = isInverted;
            SourceImage = sourceImage;
            _height = (int) sourceImage.Height;
            _width = (int) sourceImage.Width;
            _pixelsNumber = _height*_width;

            _borders = new int[_width, _height];
            _labels = new int[_width, _height];
            for(int y = 0; y < _height; y++)
            {
                for(int x = 0; x < _width; x++)
                {
                    _labels[x, y] = 0;
                }
            }

            if(treshold != null)
            {
                _treshold = (int) treshold;
                _needCalculateTreshold = false;
            }

            GreyImage = GetGreyImage();
            BinaryImage = GetBinaryImage();
            RecognizedImage = GetRecognizedImage();
        }

        public BitmapImage DefineObjects()
        {
            var random = new Random();
            int deltaColor = 200/(K - 1);
            for (int i = 0; i < K; i++)
            {
                _vectors.Add(new Vector()
                {
                    Compactness = _compactness.Values.Min() + (_compactness.Values.Max() - _compactness.Values.Min()) / 100 * random.Next(0, 100),
                    Elongation = _elongation.Values.Min() + (_elongation.Values.Max() - _elongation.Values.Min()) / 100 * random.Next(0, 100),
                    Perimeter = _perimeter.Values.Min() + (_compactness.Values.Max() - _compactness.Values.Min()) / 100 * random.Next(0, 100),
                    Square = _square.Values.Min() + (_compactness.Values.Max() - _compactness.Values.Min()) / 100 * random.Next(0, 100),
                    XCenter = _xCenter.Values.Min() + (_xCenter.Values.Max() - _xCenter.Values.Min()) / 100 * random.Next(0, 100),
                    YCenter = _yCenter.Values.Min() + (_yCenter.Values.Max() - _yCenter.Values.Min()) / 100 * random.Next(0, 100),
                    Color = Color.FromArgb(20 + deltaColor * i, 100, 100)
                });
            }

            bool flagWereChanges = true;

            var vectorsCompactness = new Dictionary<int, List<double>>();
            var vectorsElongation = new Dictionary<int, List<double>>();
            var vectorsPerimeter = new Dictionary<int, List<double>>();
            var vectorsSquare = new Dictionary<int, List<double>>();
            var vectorsXCenter = new Dictionary<int, List<double>>();
            var vectorsYCenter = new Dictionary<int, List<double>>();
            for (int i = 0; i < K; i++)
            {
                vectorsCompactness.Add(i, new List<double>());
                vectorsElongation.Add(i, new List<double>());
                vectorsPerimeter.Add(i, new List<double>());
                vectorsSquare.Add(i, new List<double>());
                vectorsXCenter.Add(i, new List<double>());
                vectorsYCenter.Add(i, new List<double>());
            }

            while (flagWereChanges)
            {
                foreach (var color in _colorsHashSet)
                {
                    if(color == 0)
                    {
                        continue;
                    }

                    var distanceForColor = new double[K];
                    for(int i =  0; i < K; i++)
                    {
                        distanceForColor[i] = GetDistance(_vectors[i], color);
                    }
                    double minDistance = distanceForColor.Min();
                    for(int i =  0; i < K; i++)
                    {
                        if(distanceForColor[i] == minDistance)
                        {
                            _vectorForObject[color] = i;

                            vectorsCompactness[i].Add(_compactness[color]);
                            vectorsElongation[i].Add(_elongation[color]);
                            vectorsPerimeter[i].Add(_perimeter[color]);
                            vectorsSquare[i].Add(_square[color]);
                            vectorsXCenter[i].Add(_xCenter[color]);
                            vectorsYCenter[i].Add(_yCenter[color]);
                            break;
                        }
                    }
                }

                for (int i = 0; i < K; i++)
                {
                    double newCompactness = vectorsCompactness[i].Any() ? vectorsCompactness[i].Average() : 0;
                    double newElongation = vectorsElongation[i].Any() ? vectorsElongation[i].Average() : 0;
                    double newPerimeter = vectorsPerimeter[i].Any() ? vectorsPerimeter[i].Average() : 0;
                    double newSquare = vectorsSquare[i].Any() ? vectorsSquare[i].Average() : 0;
                    double newXCenter = vectorsXCenter[i].Any() ? vectorsXCenter[i].Average() : 0;
                    double newYCenter = vectorsYCenter[i].Any() ? vectorsYCenter[i].Average() : 0;

                    if (Math.Abs(_vectors[i].Compactness - newCompactness) > 0.05
                        || Math.Abs(_vectors[i].Elongation - newElongation) > 0.05
                        || Math.Abs(_vectors[i].Perimeter - newPerimeter) > 0.05
                        || Math.Abs(_vectors[i].Square - newSquare) > 0.05
                        || Math.Abs(_vectors[i].XCenter - newXCenter) > 0.05
                        || Math.Abs(_vectors[i].YCenter - newYCenter) > 0.05)
                    {
                        _vectors[i].Compactness = newCompactness;
                        _vectors[i].Elongation = newElongation;
                        _vectors[i].Perimeter = newPerimeter;
                        _vectors[i].Square = newSquare;
                        _vectors[i].XCenter = newXCenter;
                        _vectors[i].YCenter = newYCenter;
                    }
                    else
                    {
                        flagWereChanges = false;
                    }
                }
            }
            
            Bitmap bitmap = BitmapImage2Bitmap(RecognizedImage);
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_labels[x, y] != 0)
                    {
                        bitmap.SetPixel(x, y, _vectors[_vectorForObject[_labels[x, y]]].Color);
                    }
                }
            }
            return Bitmap2BitmapImage(bitmap);
        }

        private double GetDistance(Vector vector, int color)
        {
            return Math.Sqrt(
                (vector.Compactness - _compactness[color]) * (vector.Compactness - _compactness[color]) +
                (vector.Elongation - _elongation[color]) * (vector.Elongation - _elongation[color]) +
                (vector.Perimeter - _perimeter[color]) * (vector.Perimeter - _perimeter[color]) +
                (vector.Square - _square[color]) * (vector.Square - _square[color]) +
                (vector.XCenter - _xCenter[color]) * (vector.XCenter - _xCenter[color]) +
                (vector.YCenter - _yCenter[color]) * (vector.YCenter - _yCenter[color])
                );
        }

        public Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return new Bitmap(bitmap);
        }

        public BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage;
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        private BitmapImage GetGreyImage()
        {
            Bitmap bitmap = BitmapImage2Bitmap(SourceImage);

            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    var hsv = new HSV(bitmap.GetPixel(j, i));
                    bitmap.SetPixel(j, i, (new HSV(0, 0, hsv.Value)).ToColor());
                }
            }

            return Bitmap2BitmapImage(bitmap);
        }

        private BitmapImage GetBinaryImage()
        {
            var brightnessesNumber = new int[256];

            Color[,] pixelsArray= GetPixelArray(BitmapImage2Bitmap(GreyImage));
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    var hsv = new HSV(pixelsArray[x, y]);
                    brightnessesNumber[hsv.Value]++;
                }
            }

            if(_needCalculateTreshold)
            {
                _treshold = brightnessesNumber.Select((t, i) => i*t).Sum()/_pixelsNumber;
            }

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    var hsv = new HSV(pixelsArray[x, y]);
                    pixelsArray[x, y] = (new HSV(0, 0, !IsInverted
                                                       ? hsv.Value >= _treshold ? (byte) 255 : (byte) 0
                                                       : hsv.Value >= _treshold ? (byte) 0 : (byte) 255)).ToColor();
                }
            }

            return Bitmap2BitmapImage(GetBitmapFromPixelsArray(pixelsArray));
        }

        private BitmapImage GetRecognizedImage()
        {
            Bitmap bitmap = BitmapImage2Bitmap(BinaryImage);

            int[,] pixelValues = GetPixelValuesArray(bitmap);

            int L = 1; // labels должна быть обнулена
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    Fill(pixelValues, x, y, L++);
                }
            }

            DrawImage(bitmap);

            return Bitmap2BitmapImage(bitmap);
        }

        private void Fill(int[,] pixelValues, int x, int y, int L)
        {
            _counter++;
            if (_counter < 6500)
            {
                if ((_labels[x, y] == 0) && pixelValues[x, y] == 0)
                {
                    _labels[x, y] = L;
                    if (x > 0)
                    {
                        Fill(pixelValues, x - 1, y, L);
                    }
                    if (x > 0 && y > 0)
                    {
                        Fill(pixelValues, x - 1, y - 1, L);
                    }
                    if (y > 0)
                    {
                        Fill(pixelValues, x, y - 1, L);
                    }
                    if (x < _width - 1 && y > 0)
                    {
                        Fill(pixelValues, x + 1, y - 1, L);
                    }
                    if (x < _width - 1)
                    {
                        Fill(pixelValues, x + 1, y, L);
                    }
                    if (x < _width - 1 && y < _height - 1)
                    {
                        Fill(pixelValues, x + 1, y + 1, L);
                    }
                    if (y < _height - 1)
                    {
                        Fill(pixelValues, x, y + 1, L);
                    }
                    if (x > 0 && y < _height - 1)
                    {
                        Fill(pixelValues, x - 1, y + 1, L);
                    }
                }
            }
            else
            {
                _borders[x, y] = 1;
            }
            _counter--;
        }

        private void DrawImage(Bitmap bitmap)
        {
            Dictionary<int, byte> colors = GetColors();

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    byte color = colors[_labels[x, y]];
                    bitmap.SetPixel(x, y, (new HSV(color != 255 ? color : (byte)0,
                                                   color != 255 ? color : (byte)0,
                                                   color)).ToColor());
                    // bitmap.SetPixel(x, y, (new HSV(0, 0, color)).ToColor());
                }
            }
        }

        private Dictionary<int, byte> GetColors()
        {
            var hashSet1 = new HashSet<int>();

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    hashSet1.Add(_labels[x, y]);
                }
            }

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int basePoint = _labels[x, y];

                    if (_borders[x, y] == 1 && basePoint != 0)
                    {
                        int curPoint = 0;

                        if (x > 0)
                        {
                            curPoint = _labels[x - 1, y];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(basePoint, curPoint);
                                continue;
                            }
                        }
                        if (x > 0 && y > 0)
                        {
                            curPoint = _labels[x - 1, y - 1];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(basePoint, curPoint);
                                continue;
                            }
                        }
                        if (y > 0)
                        {
                            curPoint = _labels[x, y - 1];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(basePoint, curPoint);
                                continue;
                            }
                        }
                        if (x < _width - 1 && y > 0)
                        {
                            curPoint = _labels[x + 1, y - 1];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(basePoint, curPoint);
                                continue;
                            }
                        }
                        if (x < _width - 1)
                        {
                            curPoint = _labels[x + 1, y];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(basePoint, curPoint);
                                continue;
                            }
                        }
                        if (x < _width - 1 && y < _height - 1)
                        {
                            curPoint = _labels[x + 1, y + 1];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(basePoint, curPoint);
                                continue;
                            }
                        }
                        if (y < _height - 1)
                        {
                            if (_labels[x, y + 1] != 0 && _labels[x, y + 1] != basePoint)
                            {
                                UniteColors(basePoint, curPoint);
                                continue;
                            }
                        }
                        if (x > 0 && y < _height - 1)
                        {
                            curPoint = _labels[x - 1, y + 1];
                            if (curPoint != 0 && curPoint != basePoint)
                            {
                                UniteColors(basePoint, curPoint);
                                continue;
                            }
                        }
                    }
                }
            }

            _colorsHashSet = new HashSet<int>();

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    bool testFlag = _colorsHashSet.Add(_labels[x, y]);
                }
            }

            var colors = new Dictionary<int, byte>();
            byte curColor = 30;
            for (int i = 0; i < _colorsHashSet.Count; i++)
            {
                int hashElement = _colorsHashSet.ElementAt(i);
                colors.Add(hashElement, (hashElement == 0) ? (byte)255 : curColor);
                curColor += 20;
                if (curColor > 230)
                {
                    curColor = 20;
                }
            }

            return colors;
        }

        private void UniteColors(int toChange, int changeBy)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_labels[x, y] == toChange)
                    {
                        _labels[x, y] = changeBy;
                    }
                }
            }
        }

        private Color[,] GetPixelArray(Bitmap bitmap)
        {
            var pixels = new Color[_width, _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    pixels[x, y] = bitmap.GetPixel(x, y);
                }
            }
            return pixels;
        }

        private Bitmap GetBitmapFromPixelsArray(Color[,] pixelsArray)
        {
            var bitmap = new Bitmap(_width, _height);
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    bitmap.SetPixel(x, y, pixelsArray[x, y]);
                }
            }
            return bitmap;
        }

        private int[,] GetPixelValuesArray(Bitmap bitmap)
        {
            var pixels = new int[_width, _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    pixels[x, y] = new HSV(bitmap.GetPixel(x, y)).Value;
                }
            }
            return pixels;
        }

        private Dictionary<int, int> GetSquare()
        {
            var square = new Dictionary<int, int>();
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_labels[x, y] == 0)
                    {
                        continue;
                    }

                    if (square.ContainsKey(_labels[x, y]))
                    {
                        square[_labels[x, y]]++;
                    }
                    else
                    {
                        square.Add(_labels[x, y], 1);
                    }
                }
            }

            return square;
        }

        private void CalculateMassCenters()
        {
            _xCenter = new Dictionary<int, double>();
            _yCenter = new Dictionary<int, double>();

            foreach (var color in _colorsHashSet)
            {
                if (color != 0)
                {
                    _xCenter.Add(color, 0);
                    _yCenter.Add(color, 0);
                }
            }

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int curLabel = _labels[x, y];
                    if (curLabel != 0)
                    {
                        _xCenter[_labels[x, y]] += x;
                        _yCenter[_labels[x, y]] += y;
                    }
                }
            }

            foreach (var color in _colorsHashSet)
            {
                if (color != 0)
                {
                    _xCenter[color] /= _square[color];
                    _yCenter[color] /= _square[color];
                }
            }
        }

        private void CalculatePerimeter()
        {
            _perimeter = new Dictionary<int, int>();
            foreach (var color in _colorsHashSet)
            {
                if(color != 0)
                {
                    _perimeter.Add(color, 0);
                }
            }

            for(int y = 0; y < _height; y++)
            {
                for(int x = 0; x < _width; x++)
                {
                    if (_labels[x, y] != 0)
                    {
                        if (IsBoundary(x, y))
                        {
                            _perimeter[_labels[x, y]]++;
                        }
                    }
                }
            }
        }

        private bool IsBoundary(int x, int y)
        {
            int curColor = _labels[x, y];
            if (x > 0)
            {
                if (curColor != _labels[x - 1, y])
                {
                    return true;
                }
            }
            if (x > 0 && y > 0)
            {
               if (curColor != _labels[x - 1, y - 1])
               {
                   return true;
               }
            }
            if (y > 0)
            {
                if (curColor != _labels[x, y - 1])
                {
                    return true;
                }
            }
            if (x < _width - 1 && y > 0)
            {
                if (curColor != _labels[x + 1, y - 1])
                {
                    return true;
                }
            }
            if (x < _width - 1)
            {
                if(curColor != _labels[x + 1, y])
                {
                    return true;
                }
            }
            if (x < _width - 1 && y < _height - 1)
            {
                if (curColor != _labels[x + 1, y + 1])
                {
                    return true;
                }
            }
            if (y < _height - 1)
            {
                if (curColor != _labels[x, y + 1])
                {
                    return true;
                }
            }
            if (x > 0 && y < _height - 1)
            {
                if (curColor != _labels[x - 1, y + 1])
                {
                    return true;
                }
            }

            return false;
        }

        private void CalculateCompactness()
        {
            foreach (var color in _colorsHashSet)
            {
                if (color != 0)
                {
                    _compactness.Add(color, _square[color] == 0 ? 0 : _perimeter[color]*_perimeter[color]/_square[color]);
                }
            }
        }

        private void CalculateElongation()
        {
            foreach (var color in _colorsHashSet)
            {
                if (color != 0)
                {
                    _m02.Add(color, 0);
                    _m11.Add(color, 0);
                    _m20.Add(color, 0);
                }
            }

            for(int y = 0; y < _height; y++)
            {
                for(int x  = 0; x < _width; x++)
                {
                    int curColor = _labels[x, y];
                    if (curColor != 0)
                    {
                        double dX = x - _xCenter[curColor];
                        double dY = y - _yCenter[curColor];

                        _m02[curColor] += CalculateDiscretCenterMoment(dX, dY, 0, 2);
                        _m11[curColor] += CalculateDiscretCenterMoment(dX, dY, 1, 1);
                        _m20[curColor] += CalculateDiscretCenterMoment(dX, dY, 2, 0);
                    }
                }
            }

            foreach (var color in _colorsHashSet)
            {
                if (color != 0)
                {
                    double val1 = _m20[color] + _m02[color];
                    double val2 =
                        Math.Sqrt((_m20[color] - _m02[color])*(_m20[color] - _m02[color]) + 4*_m11[color]*_m11[color]);
                    _elongation.Add(color, val1 - val2 != 0 ? (val1 + val2) / (val1 - val2) : 0);
                }
            }
        }

        private double CalculateDiscretCenterMoment(double dX, double dY, int i, int j)
        {
            return Math.Pow(dX, i) * Math.Pow(dY, j);
        }
    }
}