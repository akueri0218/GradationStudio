using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

namespace GS_BMP
{
    class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public static int MAX = 1024;

        public static double Distance(Position pos1, Position pos2)
        {
            return Math.Sqrt(Math.Pow(pos1.X - pos2.X, 2) + Math.Pow(pos1.Y - pos2.Y, 2) + Math.Pow(pos1.Y - pos2.Y, 2));
        }

        public Position(int x, int y, int z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    class GSColor
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public double Distance(GSColor color)
        {
            return Math.Sqrt(Math.Pow(R - color.R, 2) + Math.Pow(G - color.G, 2) + Math.Pow(B - color.B, 2));
        }
        //内積
        public int InnerProduct(GSColor color1, GSColor color2)
        {
            int R1 = color1.R - R;
            int G1 = color1.G - G;
            int B1 = color1.B - B;

            int R2 = color2.R - R;
            int G2 = color2.G - G;
            int B2 = color2.B - B;

            return R1 * R2 + G1 * G2 + B1 * B2; 
        }
        //影
        public double Projection(GSColor end, GSColor color)
        {
            return InnerProduct(end, color) / Distance(end);
        }

        public int AsInt()
        {
            return R * 256 * 256 + G * 256 + B;
        }

        public SolidColorBrush AsSolidColor()
        {
            return new SolidColorBrush(Color.FromArgb(A, R, G, B));
        }

        public static List<GSColor> Gradation(GSColor begin, GSColor end, int Length)
        {
            List<GSColor> colorList = new List<GSColor>();

            if (Length == 0) Length = 1;

            double scaleR = (double)(end.R - begin.R) / Length;
            double scaleG = (double)(end.G - begin.G) / Length;
            double scaleB = (double)(end.B - begin.B) / Length;

            for(int i = 0; i < Length; i++)
            {
                colorList.Add(new GSColor((byte)(begin.R + scaleR * i), (byte)(begin.G + scaleG * i), (byte)(begin.B + scaleB * i)));
            }

            return colorList;
        }

        public override string ToString()
        {
            return this.R.ToString("X2") + this.G.ToString("X2") + this.B.ToString("X2");
        }

        public override bool Equals(object obj)
        {
            return obj is GSColor color &&
                   R == color.R &&
                   G == color.G &&
                   B == color.B;
        }

        public override int GetHashCode()
        {
            var hashCode = -1520100960;
            hashCode = hashCode * -1521134295 + R.GetHashCode();
            hashCode = hashCode * -1521134295 + G.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            return hashCode;
        }

        public static GSColor Black
        {
            get { return new GSColor(0, 0, 0); }
        }
        public static GSColor White
        {
            get { return new GSColor(255, 255, 255); }
        }

        public GSColor(byte r = 0, byte g = 0, byte b = 0, byte a = 255)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }
    }

    class Pixel
    {
        public Position Pos { get; set; }
        public GSColor Color { get; set; }

        public Pixel(Position pos, GSColor color)
        {
            this.Pos = pos;
            this.Color = color;
        }
    }

    class BMP
    {
        public string Path { get; }

        public Pixel[] Pixels { get; private set; }

        public Pixel[] ConvertedPixels { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public double DpiX { get; set; }
        public double DpiY { get; set; }

        public int Stride { get; set; }

        public static byte[] ExportPixel(Pixel[] pixels, int width, int height)
        {
            byte[] result = new byte[width * height * 4];
            int pos;

            for(int x = 0; x < width; x++)
            {
                for(int  y = 0; y < height; y++)
                {
                    pos = (x + y * width) * 4;
                    result[pos + 0] = pixels[pos / 4].Color.B;
                    result[pos + 1] = pixels[pos / 4].Color.G;
                    result[pos + 2] = pixels[pos / 4].Color.R;
                    result[pos + 3] = pixels[pos / 4].Color.A;
                }
            }

            return result;
        }

        public void ImportPixel()
        {
            BitmapSource source = new BitmapImage(new Uri(@Path, UriKind.Relative));

            FormatConvertedBitmap fcBMP = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);
            this.Width = fcBMP.PixelWidth;
            this.Height = fcBMP.PixelHeight;
            this.DpiX = source.DpiX;
            this.DpiY = source.DpiY;

            byte[] pixels = new byte[this.Width * this.Height * 4];

            this.Stride = (this.Width * fcBMP.Format.BitsPerPixel + 7) / 8;
            fcBMP.CopyPixels(pixels, Stride, 0);

            this.Pixels = new Pixel[pixels.Length / 4];

            for(int i = 0; i < pixels.Length; i += 4)
            {
                this.Pixels[i / 4] = new Pixel(new Position(i / 4 % this.Width, i / 4 / this.Width), new GSColor(pixels[i + 2], pixels[i + 1], pixels[i + 0], pixels[i + 3]));
            }
        }

        public static void ExportImage(Pixel[] pixels, int width, int height, double dpiX, double dpiY, int stride)
        {
            if (pixels == null) return;

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "PNG file(*.png)|*.png";

            if (dialog.ShowDialog() == true)
            {
                using (FileStream filestream = new FileStream(dialog.FileName, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();

                    encoder.Interlace = PngInterlaceOption.On;

                    encoder.Frames.Add(BitmapFrame.Create(BitmapSource.Create(width, height, dpiX, dpiY, PixelFormats.Pbgra32, null, ExportPixel(pixels, width, height), stride)));
                    encoder.Save(filestream);
                }
            }
        }

        public BMP(string path)
        {
            Path = path;

            ImportPixel();
        }
    }
}
