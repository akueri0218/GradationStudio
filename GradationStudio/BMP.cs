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

        public Color(byte r = 0, byte g = 0, byte b = 0, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }

    class BMP
    {
        private Color[] pixels;

        private int width;
        private int height;

        private double dpiX;
        private double dpiY;

        private int stride;

        public Color[] Pixels
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

        public byte[] GetPixelAsByte()
        {
            byte[] result = new byte[Width * Height * 4];

            for(int i = 0; i < result.Length; i += 4)
            {
                result[i + 0] = Pixels[i / 4].B;
                result[i + 1] = Pixels[i / 4].G;
                result[i + 2] = Pixels[i / 4].R;
                result[i + 3] = Pixels[i / 4].A;
            }

            return result;
        }

        public void LoadImage(string path)
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
                this.Pixels[i / 4] = new Color(pixels[i + 2], pixels[i + 1], pixels[i + 0], pixels[i + 3]);
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
    }
}
