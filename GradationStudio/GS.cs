﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public ColorChunk(GSColor color)
        {
            Color = color;
            Count = 0;
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
        private static double CLOSENESS = 8; 

        public List<GSColor> ColorList { get; set; }
        public List<Pallet> KeyColorPallet { get; private set; }

        public List<Pallet> GetKeyColorList()
        {
            List<GSColor> queue = new List<GSColor>(ColorList);
            List<GSColor> lineColorList = new List<GSColor>();

            GSColor begin = new GSColor();
            GSColor end = new GSColor();

            List<ColorLine> lineList = new List<ColorLine>();

            Console.WriteLine("Getting key color data...");

            //1番目に数の多い色と2番目に数の多い色を通る直線を引く
            ColorLine dummy = new ColorLine(queue[0], queue[1]);

            //その直線に近い点をすべてlineColorListに追加，queueから削除
            lineColorList.AddRange(queue.FindAll(color => dummy.Distance(color) < CLOSENESS));
            queue.RemoveAll(color => lineColorList.Contains(color));

            //その直線に近い点の中で，直線の端に近い点(影が一番長い)を始点，終点にする
            //影の長さでソート 降順？
            lineColorList.OrderBy(color => dummy.Begin.Projection(dummy.End, color));

            begin = lineColorList.First();
            end = lineColorList.Last();

            //直線をlineListに追加
            lineList.Add(new ColorLine(begin, end));

            while (queue.Count > 0)
            {
                //終点を始点にしてqueueの中で1番目に数の多い色を終点とする直線を引く
                dummy = new ColorLine(end, queue[0]);

                //その直線に近い点のうちlineのBeginよりEnd側をすべてwaitListに追加，queueから削除
                lineColorList.AddRange(queue.FindAll(color => dummy.Distance(color) < CLOSENESS && dummy.Begin.Projection(dummy.End, color) >= 0));
                queue.RemoveAll(color => lineColorList.Contains(color));

                //その直線に近い点の中で，直線の端に近い点(影が一番長い)を終点にする始点は直線の始点
                //影の長さでソート 降順？
                lineColorList.OrderBy(color => dummy.Begin.Projection(dummy.End, color));

                begin = dummy.Begin;
                end = lineColorList.Last();

                //直線をlineListに追加
                lineList.Add(new ColorLine(begin, end));
            }

            //lineListの直線の長さの合計とそれぞれの直線の長さから長さの割合を出す
            //その割合に合わせてKeyColorListに追加

            Console.WriteLine("Key colors data was gotten successfully!");

            //Debug
            List<Pallet> palletList = new List<Pallet>();
            foreach (ColorLine line in lineList)
                palletList.Add(new Pallet(line.Begin));
            palletList.Add(new Pallet(lineList.Last().End));
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

            Console.WriteLine("Loading pixels...");

            foreach (Pixel pixel in pixels)
            {
                if (ChunkList.Find(chunk => chunk.Color.Equals(pixel.Color)) == null)
                {
                    ChunkList.Add(new ColorChunk(pixel.Color));
                }

                ChunkList.Find(chunk => chunk.Color.Equals(pixel.Color)).Add();
            }

            // Sort by Count
            ChunkList.Sort((a, b) => b.Count - a.Count);
            //ChunkList.OrderBy(chunk => chunk.PixelList.Count);

            ColorList = ChunkList.Select(chunk => chunk.Color).ToList();

            Console.WriteLine("Pixels was loaded successfully!");
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
