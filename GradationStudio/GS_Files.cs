using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using GS_BMP;
using GS;

namespace GS_Files
{
    class GRD
    {
        public static void Export(string filename, List<Pallet> pallets)
        {
            BinaryWriter writer = new BinaryWriter(new FileStream(filename, FileMode.Create));

            try
            {
                writer.Write('G');
                writer.Write('R');
                writer.Write(2 + 4 + 2 + pallets.Count * (2 + 1 + 1 + 1));//file size
                writer.Write((short)pallets.Count);//color count
                foreach(Pallet pallet in pallets)
                {
                    writer.Write(pallet.Pos);
                    writer.Write(pallet.Color.R);
                    writer.Write(pallet.Color.G);
                    writer.Write(pallet.Color.B);
                }
            }
            finally
            {
                writer.Close();
            }
        }

        public static List<Pallet> Load(string filename)
        {
            BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open));

            List<Pallet> result = new List<Pallet>();

            byte pos;
            byte r, g, b;

            short count;

            try
            {
                if (reader.ReadChar() != 'G')
                    return null;
                if (reader.ReadChar() != 'R')
                    return null;
                reader.ReadInt32();

                count = reader.ReadInt16();

                for(int i = 0; i < count; i++)
                {
                    pos = reader.ReadByte();
                    r = reader.ReadByte();
                    g = reader.ReadByte();
                    b = reader.ReadByte();

                    result.Add(new Pallet(new GSColor(r, g, b), pos));
                }
            }
            finally
            {
                reader.Close();
            }

            return result;
        }

        GRD()
        {

        }
    }
}
