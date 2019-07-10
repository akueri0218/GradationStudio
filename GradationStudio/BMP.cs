using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

namespace GradationStudio
{
    class Position
    {
        private int x;
        private int y;

        public int X
        {
            get { return this.x; }
            set { this.x = value; }
        }
        public int Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        public Position(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    static class ColorFormat
    {
        public const byte GRAY = 0;
        public const byte RGB = 1;
        public const byte GRAY_A = 2;
        public const byte RGBA = 3;
    }

    class Color
    {
        private byte red;
        private byte green;
        private byte blue;
        private byte alpha;

        public byte R
        {
            get { return this.red; }
            set { this.red = value; }
        }
        public byte G
        {
            get { return this.green; }
            set { this.green = value; }
        }
        public byte B
        {
            get { return this.blue; }
            set { this.blue = value; }
        }
        public byte A
        {
            get { return this.alpha; }
            set { this.alpha = value; }
        }

        public Color(byte format, byte r = 0, byte g = 0, byte b = 0, byte a = 255)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }
    }

    class Pixel
    {
        private Position pos;
        private Color color;

        public Position Pos
        {
            get { return this.pos; }
            set { this.pos = value; }
        }
        public Color Color
        {
            get { return this.color; }
            set { this.color = value; }
        }

        public Pixel(Position pos, Color color)
        {
            this.Pos = pos;
            this.Color = color;
        }
    }

    class BMP
    {
        private readonly string path;

        private Pixel[] pixels;

        private int width;
        private int height;

        private double dpiX;
        private double dpiY;

        private int stride;

        public string Path
        {
            get { return this.path; }
        }

        public Pixel[] Pixels
        {
            get { return this.pixels; }
            set { this.pixels = value; }
        }

        public int Width
        {
            get { return this.width; }
            set { this.width = value; }
        }
        public int Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        public double DpiX
        {
            get { return this.dpiX; }
            set { this.dpiX = value; }
        }
        public double DpiY
        {
            get { return this.dpiY; }
            set { this.dpiY = value; }
        }

        public int Stride
        {
            get { return this.stride; }
            set { this.stride = value; }
        }

        public byte[] ExportPixel()
        {
            byte[] result = new byte[this.Width * this.Height * 4];
            int pos;

            for(int x = 0; x < this.Width; x++)
            {
                for(int  y = 0; y < this.Height; y++)
                {
                    pos = x + y * this.Width * 4;
                    result[pos + 0] = this.Pixels[pos].Color.B;
                    result[pos + 1] = this.Pixels[pos].Color.G;
                    result[pos + 2] = this.Pixels[pos].Color.R;
                    result[pos + 3] = this.Pixels[pos].Color.A;
                }
            }

            return result;
        }

        public void ImportPixel(string path)
        {
            BitmapSource source = new BitmapImage(new Uri(@path, UriKind.Relative));

            FormatConvertedBitmap fcBMP = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);
            this.Width = fcBMP.PixelWidth;
            this.Height = fcBMP.PixelHeight;
            this.DpiX = source.DpiX;
            this.DpiY = source.DpiY;

            byte[] pixels = new byte[Width * Height * 4];

            this.Stride = (Width * fcBMP.Format.BitsPerPixel + 7) / 8;
            fcBMP.CopyPixels(pixels, stride, 0);

            for(int i = 0; i < pixels.Length; i += 4)
            {
                this.Pixels[i] = new Pixel(new Position(i % this.Width, i / this.Width), new Color(pixels[i + 2], pixels[i + 1], pixels[i + 0], pixels[i + 3]));
            }
        }

        public void ExportImage()
        {
            if (this.Pixels == null) return;

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "PNG file(*.png)|*.png";

            if (dialog.ShowDialog() == true)
            {
                using (FileStream filestream = new FileStream(dialog.FileName, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();

                    encoder.Interlace = PngInterlaceOption.On;

                    encoder.Frames.Add(BitmapFrame.Create(BitmapSource.Create(this.Width, this.Height, this.DpiX, this.DpiY, PixelFormats.Pbgra32, null, GetPixelAsByte(), this.Stride)));
                    encoder.Save(filestream);
                }
            }
        }

        public BMP(string path)
        {
            ImportPixel(path);
        }
    }
}
