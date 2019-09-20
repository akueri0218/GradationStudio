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
using GS;

namespace GradationStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BMP bmp = new BMP("../../img/bosco.jpg");
        //private BMP source_bmp = new BMP("../../img/lena.png");
        private ColorMap map;
        //private ColorMap source_map;

        private Gradation gradation;
        //private Gradation source_gradation;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void GSInit()
        {
            map = new ColorMap(bmp.Pixels);
            //source_map = new ColorMap(source_bmp.Pixels);

            await Task.Run(() => { gradation = new Gradation(map.ColorList); });
            //source_gradation = new Gradation(source_map.ChunkColorList);

            Task.WaitAll();
            
            List<GSColor> colorList = Gradation.MakeGradation(gradation.KeyColorPallet);

            colorList.ForEach(color =>
            {
                Label label = new Label();
                //label.Content = ((double)map.ChunkList[map.ChunkColorList.IndexOf(color)].PixelList.Count / (bmp.Width * bmp.Height) * 100).ToString("F5") + "%";
                label.Background = color.AsSolidColor();
                //if ((color.R + color.G + color.B) / 3 < 64) label.Foreground = Brushes.White;
                label.HorizontalContentAlignment = HorizontalAlignment.Right;
                label.Padding = new Thickness(0, 20, 10, 0);
                label.Margin = new Thickness(0, 0, 0, 0);
                label.Height = 3;
                PaletteGrid.Children.Add(label);
            });
        }

        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            btn.IsEnabled = false;



            GSInit();
        }
    }
}
