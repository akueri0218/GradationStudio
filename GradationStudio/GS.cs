using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradationStudio
{
    class Palette
    {
        private Color color;
        private byte pos;

        public Color Color
        {
            get { return this.color; }
            set { this.color = value; }
        }
        public byte Pos
        {
            get { return this.pos; }
            private set { this.pos = value; }
        }

        public Palette(Color color, byte pos)
        {
            this.Color = color;
            this.Pos = pos;
        }
    }

    class ColorChunk
    {
        public static readonly byte SIZE = 8;

        private readonly Color start;
        private readonly Color end;

        private Pixel[] pixels;

        public Color Start
        {
            get { return this.start; }
        }
        public Color End
        {
            get { return this.end; }
        }

        public Pixel[] Pixels
        {
            get { return this.pixels; }
            set { this.pixels = value; }
        }

        private bool Between(byte num, byte max, byte min)
        {
            return max >= num && num >= min;
        }

        private bool IsContain(Pixel pixel)
        {
            Color color = pixel.Color;
            return Between(color.R, this.Start.R, this.End.R) && Between(color.G, this.Start.G, this.End.G) && Between(color.B, this.Start.B, this.End.B);
        }

        public Pixel[] Contain(Pixel[] pixels)
        {
            return Array.FindAll(pixels, IsContain);
        }

        private void AddPixel(Pixel pixel)
        {
            Pixel[] temp = this.Pixels;
            int length = this.Pixels.Length;

            this.Pixels = new Pixel[length + 1];
            Array.Copy(temp, this.Pixels, length);

            this.Pixels[length] = pixel;
        }
        public void AddPixels(Pixel[] pixels)
        {
            foreach(Pixel pixel in Contain(this.Pixels))
            {
                AddPixel(pixel);
            }
        }

        public ColorChunk(Color start, Color end, Pixel[] pixels)
        {
            this.start = start;
            this.end = end;

            this.Pixels = new Pixel[0];
            AddPixels(pixels);
        }
    }

    class ColorMap3D : BMP
    {
        private ColorChunk[,,] chunks;
        private Palette[] palettes;
        private BMP gray;

        public ColorChunk[,,] Chunks
        {
            get { return this.chunks; }
            private set { this.chunks = value; }
        }
        public Palette[] Palettes
        {
            get { return this.palettes; }
            set { this.palettes = value; }
        }
        public BMP Gray
        {
            get { return this.gray; }
            set { this.gray = value; }
        }

        public ColorMap3D(BMP image) : base(image.Path)
        {
            for(byte r = 0; r < 256 / ColorChunk.SIZE; r++)
            {
                for (byte g = 0; g < 256 / ColorChunk.SIZE; g++)
                {
                    for (byte b = 0; b < 256 / ColorChunk.SIZE; b++)
                    {
                        Color start = new Color((byte)(r * ColorChunk.SIZE), (byte)(g * ColorChunk.SIZE), (byte)(b * ColorChunk.SIZE));
                        Color end = new Color((byte)((r + 1) * ColorChunk.SIZE - 1), (byte)((g + 1) * ColorChunk.SIZE - 1), (byte)((b + 1) * ColorChunk.SIZE - 1));
                        Chunks[r, g, b] = new ColorChunk(start, end);
                    }
                }
            }


        }
    }

    class GS
    {

    }
}
