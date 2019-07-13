using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradationStudio
{
    class Palette
    {
        private GSColor color;
        private byte pos = 0;

        public GSColor Color
        {
            get { return this.color; }
            set { this.color = value; }
        }
        public byte Pos
        {
            get { return this.pos; }
            set { this.pos = value; }
        }

        public Palette(GSColor color)
        {
            this.Color = color;
        }
    }

    class ColorChunk
    {
        public static readonly byte SIZE = 8;

        private Position pos;
        private Pixel[] pixels;

        public Position Pos
        {
            get { return this.pos; }
            set { this.pos = value; }
        }
        public Pixel[] Pixels
        {
            get { return this.pixels; }
            set { this.pixels = value; }
        }

        public void AddPixel(Pixel pixel)
        {
            if(this.Pixels == null)
            {
                this.Pixels = new Pixel[1] { pixel };
                return;
            }

            Pixel[] temp = this.Pixels;

            this.Pixels = new Pixel[temp.Length + 1];
            Array.Copy(temp, this.Pixels, temp.Length);

            this.Pixels[temp.Length] = pixel;
        }
        public void AddPixels(Pixel[] pixels)
        {
            if (pixels == null) return;

            foreach (Pixel pixel in this.Pixels)
            {
                AddPixel(pixel);
            }
        }

        public GSColor AverageColor()
        {
            if (this.Pixels == null) return null;
            if (this.Pixels.Length < ColorChunk.SIZE * ColorChunk.SIZE * ColorChunk.SIZE * 0.5) return null;

            int avgR = 0;
            int avgG = 0;
            int avgB = 0;

            foreach (Pixel pixel in this.Pixels)
            {
                avgR += pixel.Color.R;
                avgG += pixel.Color.G; 
                avgB += pixel.Color.B;
            }

            avgR /= this.Pixels.Length;
            avgR /= this.Pixels.Length;
            avgR /= this.Pixels.Length;

            return new GSColor((byte)avgR, (byte)avgG, (byte)avgB);
        }

        public static ColorChunk[] Push(ColorChunk[] chunks, ColorChunk chunk)
        {
            if(chunks == null)
                return new ColorChunk[1] { chunk };

            ColorChunk[] temp = chunks;
            chunks = new ColorChunk[chunks.Length + 1];

            Array.Copy(temp, chunks, temp.Length);
            chunks[chunks.Length - 1] = chunk;

            return chunks;
        }

        public ColorChunk(Position pos)
        {
            this.Pos = pos;
        }
    }

    class ColorChunks
    {
        private ColorChunk[] chunks;

        public ColorChunk[] Chunks
        {
            get { return this.chunks; }
            set { this.chunks = value; }
        }

        private bool DistributeColor(ColorChunk chunk, GSColor color)
        {
            if (chunk == null)
                return false;
            bool R = chunk.Pos.X == color.R / ColorChunk.SIZE;
            bool G = chunk.Pos.Y == color.G / ColorChunk.SIZE;
            bool B = chunk.Pos.Z == color.B / ColorChunk.SIZE;

            return R && G && B ? true : false;

        }
        public void AddPixels(Pixel[] pixels)
        {
            foreach(Pixel pixel in pixels)
            {
                if (this.Chunks == null)
                    this.Chunks = ColorChunk.Push(this.Chunks ,new ColorChunk(new Position(pixel.Color.R / ColorChunk.SIZE, pixel.Color.G / ColorChunk.SIZE, pixel.Color.B / ColorChunk.SIZE)));
                else if (Array.Find(this.Chunks, chunk => DistributeColor(chunk, pixel.Color)) == null)
                    this.Chunks = ColorChunk.Push(this.Chunks ,new ColorChunk(new Position(pixel.Color.R / ColorChunk.SIZE, pixel.Color.G / ColorChunk.SIZE, pixel.Color.B / ColorChunk.SIZE)));

                Array.Find(this.Chunks, chunk => DistributeColor(chunk, pixel.Color)).AddPixel(pixel);
            }
        }

        public GSColor[] AverageColors()
        {
            GSColor[] result = null;
            GSColor[] temp;

            foreach (ColorChunk chunk in this.Chunks)
            {
                if (chunk.AverageColor() != null)
                {
                    if (result == null)
                        result = new GSColor[1] { chunk.AverageColor() };
                    else
                    {
                        temp = result;
                        result = new GSColor[temp.Length + 1];
                        Array.Copy(temp, result, temp.Length);

                        result[temp.Length] = chunk.AverageColor();
                    }
                }
            }

            return result;
        }

        public ColorChunks()
        {

        }
    }

    class ColorMap3D
    {
        private BMP image;
        private ColorChunks chunks;
        private Palette[] palettes;
        private BMP gray;

        public BMP Image
        {
            get { return this.image; }
            set { this.image = value; }
        }
        public ColorChunks Chunks
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

        public ColorMap3D(BMP image)
        {
            this.Image = image;
            this.Chunks = new ColorChunks();

            this.Chunks.AddPixels(this.Image.Pixels);
        }
    }

    class GS
    {

    }
}
