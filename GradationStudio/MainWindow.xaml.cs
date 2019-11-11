using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GS_BMP;
using GS_Files;
using GS;

namespace GradationStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BMP bmp = new BMP(@"C:\Windows\Fujitsu\wallpaper\2560-1440\FJPC003.png");
        private ColorMap map;

        //private Gradation gradation;

        public MainWindow()
        {
            InitializeComponent();

            GradationDesigner designer = new GradationDesigner();
            designer.Show();
        }

        private void GSInit()
        {
            byte[] rg = new byte[256 * 256];
            byte[] gb = new byte[256 * 256];
            byte[] br = new byte[256 * 256];

            for(int i = 0; i < 256 * 256; i++)
            {
                rg[i] = 255;
                gb[i] = 255;
                br[i] = 255;
            }


            map = new ColorMap(bmp.Pixels);

            List<GSColor> colorList = new List<GSColor>(map.ColorList);

            colorList.ForEach(color =>
            {
                rg[color.R + color.G * 256] -= 64;
                gb[color.G + color.B * 256] -= 64;
                br[color.B + color.R * 256] -= 64;
            });

            BitmapSource rg_img = BitmapSource.Create(256, 256, 350, 350, PixelFormats.Gray8, null, rg, 256);
            BitmapSource gb_img = BitmapSource.Create(256, 256, 350, 350, PixelFormats.Gray8, null, gb, 256);
            BitmapSource br_img = BitmapSource.Create(256, 256, 350, 350, PixelFormats.Gray8, null, br, 256);

            RG.Source = rg_img;
            GB.Source = gb_img;
            BR.Source = br_img;

            ////source_map = new ColorMap(source_bmp.Pixels);

            //gradation = new Gradation(map.ColorList);
            ////source_gradation = new Gradation(source_map.ChunkColorList);

            //List<GSColor> colorList = Gradation.MakeGradation(gradation.KeyColorPallet);

            //colorList.ForEach(color =>
            //{
            //    Label label = new Label();
            //    //label.Content = ((double)map.ChunkList[map.ChunkColorList.IndexOf(color)].PixelList.Count / (bmp.Width * bmp.Height) * 100).ToString("F5") + "%";
            //    label.Background = color.AsSolidColor();
            //    //if ((color.R + color.G + color.B) / 3 < 64) label.Foreground = Brushes.White;
            //    label.HorizontalContentAlignment = HorizontalAlignment.Right;
            //    label.Padding = new Thickness(0, 20, 10, 0);
            //    label.Margin = new Thickness(0, 0, 0, 0);
            //    label.Height = 3;
            //    PaletteGrid.Children.Add(label);
            //});
        }

        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            btn.IsEnabled = false;

            GSInit();
        }
    }
}
