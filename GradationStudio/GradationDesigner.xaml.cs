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
using System.Windows.Shapes;

using Microsoft.Win32;

using GS_BMP;
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
    /// Interaction logic for GradationDesigner.xaml
    /// </summary>
    public partial class GradationDesigner : Window
    {
        List<ColorIndex> colorList = new List<ColorIndex>();
        Pallet pallet = new Pallet(new GSColor(255, 255, 255), 0);

        WriteableBitmap SourceImage;
        OpenFileDialog fileDialog;

        public GradationDesigner()
        {
            InitializeComponent();
        }

        private void color_Delete(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            DockPanel parent = (DockPanel)button.Parent;
            Border border = parent.Children.OfType<Border>().First();
            byte offset = 0;
            foreach(Label label in parent.Children.OfType<Label>())
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
            ColorIndex color = new ColorIndex(pallet, color_Delete);

            colorList.Add(color);
            colorIndex.Children.Add(color.Panel);

            pallet = new Pallet(new GSColor(255, 255, 255), 0);

            editR.Text = "255";
            editR.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            editG.Text = "255";
            editG.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            editB.Text = "255";
            editB.Background = new SolidColorBrush(Color.FromRgb(0, 0, 255));

            editColor_Changed();

            UpdateGradation();
        }

        private void editR_Changed(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text == "")
                return;

            int output;

            if (!int.TryParse(textBox.Text, out output))
                return;

            if (output >= 256)
                return;

            byte r = byte.Parse(textBox.Text);


            textBox.Background = new SolidColorBrush(Color.FromRgb(r, 0, 0));

            pallet.Color.R = r;

            editColor_Changed();
        }

        private void editG_Changed(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text == "")
                return;

            int output;

            if (!int.TryParse(textBox.Text, out output))
                return;

            if (output >= 256)
                return;

            byte g = byte.Parse(textBox.Text);


            textBox.Background = new SolidColorBrush(Color.FromRgb(0, g, 0));

            pallet.Color.G = g;

            editColor_Changed();
        }

        private void editB_Changed(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text == "")
                return;

            int output;

            if (!int.TryParse(textBox.Text, out output))
                return;

            if (output >= 256)
                return;

            byte b = byte.Parse(textBox.Text);

            textBox.Background = new SolidColorBrush(Color.FromRgb(0, 0, b));

            pallet.Color.B = b;

            editColor_Changed();
        }

        private void editColor_Changed()
        {
            editColor.Background = new SolidColorBrush(Color.FromRgb(pallet.Color.R, pallet.Color.G, pallet.Color.B));
        }

        private void editOffset_Changed(object sender, TextChangedEventArgs e)
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
            string filename = OpenDialog("gradation files (*.grd)|*.grd");
        }

        private void GRDSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GRDApply_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ImgOpen_Click(object sender, RoutedEventArgs e)
        {
            string filename = OpenDialog("image files (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp");



            image.Source = new BitmapImage(new Uri(@filename));
        }

        private string OpenDialog(string filter)
        {
            fileDialog = new OpenFileDialog();

            fileDialog.Filter = filter;

            if (fileDialog.ShowDialog() == false)
                return null;

            return fileDialog.FileName;
        }
    }
}
