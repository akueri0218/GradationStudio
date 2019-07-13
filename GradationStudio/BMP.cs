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
        private int z;

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
        public int Z
        {
            get { return this.z; }
            set { this.z = value; }
        }

        public Position(int x, int y, int z = 0)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }

    class GSColor
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

        public override string ToString()
        {
            return this.R.ToString("X2") + this.G.ToString("X2") + this.B.ToString("X2");
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
        private Position pos;
        private GSColor color;

        public Position Pos
        {
            get { return this.pos; }
            set { this.pos = value; }
        }
        public GSColor Color
        {
            get { return this.color; }
            set { this.color = value; }
        }

        public Pixel(Position pos, GSColor color)
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
                    result[pos + 0] = this.Pixels[pos / 4].Color.B;
                    result[pos + 1] = this.Pixels[pos / 4].Color.G;
                    result[pos + 2] = this.Pixels[pos / 4].Color.R;
                    result[pos + 3] = this.Pixels[pos / 4].Color.A;
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

            byte[] pixels = new byte[Width * Height * 4];

            this.Stride = (Width * fcBMP.Format.BitsPerPixel + 7) / 8;
            fcBMP.CopyPixels(pixels, stride, 0);

            this.Pixels = new Pixel[pixels.Length / 4];

            for(int i = 0; i < pixels.Length; i += 4)
            {
                this.Pixels[i / 4] = new Pixel(new Position(i % this.Width, i / this.Width), new GSColor(pixels[i + 2], pixels[i + 1], pixels[i + 0], pixels[i + 3]));
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

                    encoder.Frames.Add(BitmapFrame.Create(BitmapSource.Create(this.Width, this.Height, this.DpiX, this.DpiY, PixelFormats.Pbgra32, null, ExportPixel(), this.Stride)));
                    encoder.Save(filestream);
                }
            }
        }

        public BMP(string path)
        {
            this.path = path;

            ImportPixel();
        }
    }
}
