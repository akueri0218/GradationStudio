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

using Microsoft.Win32;

using GS_BMP;
using GS_Files;
using GS;

namespace GradationStudio
{
    class ColorIndex
    {
        public Pallet Pallet { get; set; }
        public DockPanel Panel { get; set; }

        public void SetPanel(Action<object, RoutedEventArgs> func)
        {
            Panel = new DockPanel();
            Panel.Height = 30;

            Border border = new Border();
            border.Width = 30;
            border.BorderThickness = new Thickness(2);
            border.BorderBrush = Brushes.Black;
            border.Background = new SolidColorBrush(Color.FromRgb(Pallet.Color.R, Pallet.Color.G, Pallet.Color.B));

            Label colorLabel = new Label();
            colorLabel.Width = 120;
            colorLabel.Content = Pallet.Color.ToString();

            Label offsetLabel = new Label();
            offsetLabel.Width = 70;
            offsetLabel.Content = Pallet.Pos.ToString();

            Button button = new Button();
            button.Width = 30;
            button.Height = 30;
            button.Content = "X";
            button.Click += new RoutedEventHandler(func);

            Panel.Children.Add(border);
            Panel.Children.Add(colorLabel);
            Panel.Children.Add(offsetLabel);
            Panel.Children.Add(button);
        }

        public ColorIndex(Pallet pallet, Action<object, RoutedEventArgs> func)
        {
            Pallet = pallet;
            SetPanel(func);
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<ColorIndex> colorList = new List<ColorIndex>();
        Pallet pallet = new Pallet(new GSColor(255, 255, 255), 0);

        WriteableBitmap SourceImage;
        OpenFileDialog fileDialog;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void colorDelete(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            DockPanel parent = (DockPanel)button.Parent;
            Border border = parent.Children.OfType<Border>().First();
            byte offset = 0;
            foreach (Label label in parent.Children.OfType<Label>())
                byte.TryParse((string)label.Content, out offset);

            GSColor color = new GSColor(((SolidColorBrush)border.Background).Color.R, ((SolidColorBrush)border.Background).Color.G, ((SolidColorBrush)border.Background).Color.B);

            colorList.RemoveAll(item => item.Pallet.Color.Equals(color) && item.Pallet.Pos == offset);

            colorIndex.Children.Clear();
            colorList.ForEach(item =>
            {
                colorIndex.Children.Add(item.Panel);
            });

            UpdateGradation();
        }
        private void AddColor_Click(object sender, RoutedEventArgs e)
        {
            ColorIndex color = new ColorIndex(pallet, colorDelete);

            colorList.Add(color);
            colorIndex.Children.Add(color.Panel);

            pallet = new Pallet(new GSColor(255, 255, 255), 0);

            UpdateGradation();
        }

        private void colorChanged(object sender, RoutedEventArgs e)
        {

        }

        private void offsetChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text == "")
                return;

            int output;

            if (!int.TryParse(textBox.Text, out output))
                return;

            if (output >= 256)
                return;

            pallet.Pos = byte.Parse(textBox.Text);
        }

        private void UpdateGradation()
        {
            colorList.OrderBy(item => item.Pallet.Pos);


            LinearGradientBrush gradient = new LinearGradientBrush();
            gradient.StartPoint = new Point(0, 0);
            gradient.EndPoint = new Point(0, 1);

            colorList.ForEach(item =>
            {
                GradientStop stop = new GradientStop();
                stop.Color = Color.FromRgb(item.Pallet.Color.R, item.Pallet.Color.G, item.Pallet.Color.B);
                stop.Offset = (double)item.Pallet.Pos / 255;

                gradient.GradientStops.Add(stop);
            });


            grdBar.Fill = gradient;
        }

        private void GRDLoad_Click(object sender, RoutedEventArgs e)
        {
            string filename = FileDialog("gradation files (*.grd)|*.grd");
        }

        private void GRDSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GRDApply_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ImgOpen_Click(object sender, RoutedEventArgs e)
        {
            string filename = FileDialog("image files (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp");

            SourceImage = new WriteableBitmap(new BitmapImage(new Uri(@filename)));

            image.Source = SourceImage;
        }

        private string FileDialog(string filter)
        {
            fileDialog = new OpenFileDialog();

            fileDialog.Filter = filter;

            if (fileDialog.ShowDialog() == false)
                return null;

            return fileDialog.FileName;
        }
    }
}
