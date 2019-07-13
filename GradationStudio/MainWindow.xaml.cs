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

namespace GradationStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ColorMap3D colorMap = new ColorMap3D(new BMP("../../lena.png"));

        public MainWindow()
        {
            InitializeComponent();

            foreach(GSColor color in colorMap.Chunks.AverageColors())
            {
                Console.WriteLine(color.ToString());

                Rectangle rectangle = new Rectangle();
                rectangle.Fill = new SolidColorBrush(Color.FromArgb(255, color.B, color.G, color.R));
                PaletteGrid.Children.Add(rectangle);
            }
        }
    }
}
