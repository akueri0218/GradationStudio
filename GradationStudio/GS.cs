using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using GS_BMP;

namespace GS
{
    /// <summary>
    /// 3次元色空間における色のチャンク
    /// </summary>
    class ColorChunk
    {
        public GSColor Color { get; set; }
        public int Count { get; set; }

        public void Add()
        {
            Count++;
        }
        public ColorChunk(GSColor color, int count = 0)
        {
            Color = color;
            Count = count;
        }
    }

    class ColorLine
    {
        public GSColor Begin { get; set; }
        public GSColor End { get; set; }

        public double Length { get { return Begin.Distance(End); } }

        public double Distance(GSColor color)
        {
            return color.Distance(new GSColor(
                (byte)(Begin.R + (Begin.Distance(color) / Length * (End.R - Begin.R))),
                (byte)(Begin.G + (Begin.Distance(color) / Length * (End.G - Begin.G))),
                (byte)(Begin.B + (Begin.Distance(color) / Length * (End.B - Begin.B)))));
        }

        public ColorLine(GSColor begin, GSColor end)
        {
            Begin = begin;
            End = end;
        }
    }

    class Pallet
    {
        public GSColor Color { get; set; }
        public byte Pos { get; set; }

        public Pallet(GSColor color, byte pos = 0)
        {
            Color = color;
            Pos = pos;
        }
    }

    class Gradation
    {
        //近さの閾値
        private static double CLOSENESS = 64; 

        public List<GSColor> ColorList { get; set; }
        public List<Pallet> KeyColorPallet { get; private set; }

        public static List<GSColor> MakeGradation(List<Pallet> palletList)
        {
            List<GSColor> colorList = new List<GSColor>();

            for(int i = 1; i < palletList.Count; i++)
            {
                colorList.AddRange(GSColor.Gradation(palletList[i - 1].Color, palletList[i].Color, palletList[i].Pos));
            }
            colorList.Add(palletList.Last().Color);

            return colorList;
        }

        public List<Pallet> GetKeyColorList()
        {
            List<GSColor> queue = new List<GSColor>(ColorList);
            List<GSColor> buffer = new List<GSColor>();
            List<GSColor> lineColorList = new List<GSColor>();

            GSColor begin = new GSColor();
            GSColor end = new GSColor();

            List<ColorLine> lineList = new List<ColorLine>();

            int i;

            Stopwatch stopwatch = new Stopwatch();

            Console.WriteLine("Getting key color data...");

            stopwatch.Start();

            ColorLine dummy = new ColorLine(queue[0], queue[1]);

            for (i = 0; i < queue.Count && !lineColorList.Any(); i++)
            {
                //1番目に数の多い色と2番目に数の多い色を通る直線を引く
                dummy = new ColorLine(queue[0], queue[i]);

                //その直線に近い点をすべてlineColorListに追加，queueから削除
                lineColorList.AddRange(queue.FindAll(color => dummy.Distance(color) < CLOSENESS));
            } 

            queue.RemoveAll(color => lineColorList.Contains(color));

            //その直線に近い点の中で，直線の端に近い点(影が一番長い)を始点，終点にする
            //影の長さでソート 降順？
            lineColorList = new List<GSColor>(lineColorList.OrderBy(color => dummy.Begin.Projection(dummy.End, color)));

            begin = lineColorList.First();
            end = lineColorList.Last();

            //直線をlineListに追加
            lineList.Add(new ColorLine(begin, end));

            int count = 0;

            while (queue.Any())
            {
                //終点を始点にしてqueueの中で1番に遠い色を終点とする直線を引く
                dummy = new ColorLine(end, queue.OrderBy(color => end.Distance(color)).First());

                //lineのBeginよりEnd側をlineColorListに追加
                queue.OrderBy(color => dummy.Begin.Projection(dummy.End, color));
                for(i = 0; i < queue.Count - 1; i++)
                {
                    if (dummy.Begin.Projection(dummy.End, queue[i]) < 0)
                        break;
                }
                lineColorList = new List<GSColor>(queue.GetRange(0, i));

                Console.WriteLine("count: " + lineColorList.Count);

                //lineに近い点のみwaitListに追加，queueから削除
                lineColorList = new List<GSColor>(lineColorList.OrderBy(color => -dummy.Distance(color)));

                for (i = 0; i < lineColorList.Count - 1; i++)
                {
                    if (dummy.Distance(lineColorList[i]) >= CLOSENESS)
                        break;
                }
                lineColorList.RemoveRange(i, lineColorList.Count - i);
                queue.RemoveAll(color => lineColorList.Contains(color));

                Console.WriteLine("Count: " + lineColorList.Count);

                //その直線に近い点の中で，直線の端に近い点(影が一番長い)を終点にする始点は直線の始点
                //影の長さでソート 降順？
                lineColorList = new List<GSColor>(lineColorList.OrderBy(color => dummy.Begin.Projection(dummy.End, color)));

                begin = dummy.Begin;
                try
                {
                    end = lineColorList.Last();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                //直線をlineListに追加
                lineList.Add(new ColorLine(begin, end));

                count++;
            }

            //lineListの直線の長さの合計とそれぞれの直線の長さから長さの割合を出す
            //その割合に合わせてKeyColorListに追加

            stopwatch.Stop();

            Console.WriteLine("Key colors data was gotten successfully!");
            Console.WriteLine("Elapsed: " + stopwatch.Elapsed.ToString());
            Console.WriteLine("CaluculateTime: " + count.ToString());

            double LengthSum = lineList.Sum(line => line.Length);

            List<Pallet> palletList = new List<Pallet>();

            palletList.Add(new Pallet(lineList.First().Begin, 0));
            lineList.ForEach(line =>
            {
                Console.WriteLine((int)(line.Length / LengthSum * 255));
                palletList.Add(new Pallet(line.End, (byte)(line.Length / LengthSum * 255)));
            });
            return palletList;
        }

        //private void Sort_BU()
        //{
        //    List<GSColor> queue = new List<GSColor>(/*KeyColorList*/);
        //    KeyColorList.Clear();
        //    GSColor begin = new GSColor();
        //    GSColor end = new GSColor();

        //    List<GSColor> waitList = new List<GSColor>();
        //    ColorLine line;

        //    while (queue.Count > 1)
        //    {
        //        begin = queue[0];

                
        //        end = queue[1];

        //        waitList.Clear();

        //        line = new ColorLine(begin, end);

        //        KeyColorList.Add(begin);
        //        KeyColorList.Add(end);
        //        queue.Remove(begin);
        //        queue.Remove(end);

        //        foreach (GSColor color in queue)
        //        {
        //            if (line.Distance(color) < 32)
        //            {
        //                waitList.Add(color);
        //            }
        //        }

        //        if (waitList.Count > 0)
        //        {
        //            foreach (GSColor color in waitList)
        //            {
        //                queue.Remove(color);
        //            }

        //            waitList.OrderBy(color => begin.InnerProduct(end, color));

        //            KeyColorList.InsertRange(KeyColorList.IndexOf(KeyColorList.Last()), waitList);
        //        }
        //    }
        //}

        public Gradation(List<GSColor> colorList)
        {
            ColorList = new List<GSColor>(colorList);

            KeyColorPallet = GetKeyColorList();
        }
    }

    class ColorMap
    {
        public List<ColorChunk> ChunkList { get; private set; }
        public List<GSColor> ColorList { get; private set; }

        private void LoadPixels(Pixel[] pixels)
        {
            if (ChunkList == null) ChunkList = new List<ColorChunk>();

            List<Pixel> pixelList = new List<Pixel>(pixels);

            Stopwatch stopwatch = new Stopwatch();

            Console.WriteLine("Loading pixels...");

            int count = 0;

            stopwatch.Start();

            pixelList = new List<Pixel>(pixelList.OrderBy(pixel => pixel.Color.AsInt()));

            int i;

            while (pixelList.Any())
            {
                for(i = 0; i < pixelList.Count - 1; i++)
                {
                    if (!pixelList[i].Color.Equals(pixelList[i + 1].Color))
                        break;
                }

                ChunkList.Add(new ColorChunk(pixelList.First().Color, i + 1));

                pixelList.RemoveRange(0, i + 1);

                count++;
            }

            stopwatch.Restart();

            // Sort by Count
            ChunkList = new List<ColorChunk>(ChunkList.OrderBy(chunk => chunk.Count));

            ColorList = ChunkList.Select(chunk => chunk.Color).ToList();

            stopwatch.Stop();

            Console.WriteLine("Pixels was loaded successfully!");
            Console.WriteLine("Elapsed: " + stopwatch.Elapsed.ToString());
            Console.WriteLine("CaluculateTime: " + count.ToString());
        }

        public List<int> getMaxIndex(List<ColorChunk> chunks)
        {
            List<int> max = new List<int> { 0 };

            foreach(ColorChunk chunk in chunks)
            {
                if(chunks[max[0]].Count <= chunk.Count)
                {
                    if (chunks[max[0]].Count < chunk.Count)
                    {
                        max.Clear();
                    }

                    max.Add(chunks.IndexOf(chunk));
                }
            }
            return max;
        }

        public ColorMap(Pixel[] pixels)
        {
            LoadPixels(pixels);
        }
    }
}
