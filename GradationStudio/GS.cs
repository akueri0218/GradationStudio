using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradationStudio
{
    class ColorMap3D
    {
        private Color[] colors = { };
        private int[] counts = { };

        public Color[] Colors
        {
            get { return this.colors; }
            private set { this.colors = value; }
        }
        public int[] Counts
        {
            get { return this.counts; }
            set { this.counts = value; }
        }

        public ColorMap3D()
        {

        }

        public void AddColor(Color color)
        {
            if (Colors.Contains(color))
            {
                int index = Array.IndexOf(Colors, color);
                Counts[index]++;

                return;
            }

            Color[] result = { };

            Array.Copy(Colors, result, Colors.Length);
            result[Colors.Length] = color;

            Array.Copy(Counts, Counts, Colors.Length);
            Counts[Colors.Length] = 0;

            Colors = result;
        }

        public void AddColorFromImage(BMP image)
        {
            int pos;

            for(int x = 0; x < image.Height; x++)
            {
                for(int y = 0; y < image.Width; y++)
                {
                    pos = x + y * image.Width;

                    AddColor(image.Pixels[pos]);
                }
            }
        }

        public string[,] getColorList()
        {
            string[,] result = new string[Colors.Length, 2];

            for (int i = 0; i < Colors.Length; i++)
            {
                result[i, 0] = Colors[i].A.ToString("X2") + Colors[i].R.ToString("X2") + Colors[i].G.ToString("X2") + Colors[i].B.ToString("X2");
                result[i, 1] = Counts[i].ToString();
            }

            return result;
        }
    }

    class GS
    {

    }
}
