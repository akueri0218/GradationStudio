using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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

        BMP SourceImage;

        BitmapSource ResultImage;

        bool isSaved = true;

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
            colorList.ForEach(item => colorIndex.Children.Add(item.Panel));

            UpdateGradation();
        }
        private void AddColor_Click(object sender, RoutedEventArgs e)
        {
            if (colorList.Find(item => item.Pallet.Pos == pallet.Pos) != null)
                return;

            ColorIndex color = new ColorIndex(pallet, colorDelete);

            colorList.Add(color);

            colorList.Sort((a, b) => a.Pallet.Pos - b.Pallet.Pos);

            colorIndex.Children.Clear();
            colorList.ForEach(item => colorIndex.Children.Add(item.Panel));

            pallet = new Pallet(new GSColor((byte)SliderR.Value, (byte)SliderG.Value, (byte)SliderB.Value), pallet.Pos);

            UpdateGradation();
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
            string filename = FileDialog_Open("gradation files (*.grd)|*.grd");

            if (filename == null)
                return;

            List<Pallet> pallets = GRD.Load(filename);

            colorList.Clear();
            colorIndex.Children.Clear();

            pallets.ForEach(item => colorList.Add(new ColorIndex(item, colorDelete)));

            colorList.Sort((a, b) => a.Pallet.Pos - b.Pallet.Pos);

            colorIndex.Children.Clear();
            colorList.ForEach(item => colorIndex.Children.Add(item.Panel));

            UpdateGradation();
        }

        private void GRDSave_Click(object sender, RoutedEventArgs e)
        {
            string filename = FileDialog_Save("gradation files (*.grd)|*.grd");

            if (filename == null)
                return;

            List<Pallet> pallets = new List<Pallet>();

            colorList.ForEach(item =>
            {
                pallets.Add(item.Pallet);
            });

            GRD.Export(filename, pallets);
        }

        private GSColor getGradationColor(Pixel pixel)
        {
            colorList.Sort((a, b) => a.Pallet.Pos - b.Pallet.Pos);

            Pallet p1 = colorList.First().Pallet;
            Pallet p2 = colorList.First().Pallet;

            byte index = (byte)Math.Round(pixel.Color.R * 0.2126 + pixel.Color.G * 0.7152 + pixel.Color.B * 0.0722);

            if (pixel.Color.A == 0)
                return pixel.Color;

            byte R;
            byte G;
            byte B;

            for (int i = 0; i < colorList.Count; i++)
            {
                if (colorList[i].Pallet.Pos == index)
                    return new GSColor(colorList[i].Pallet.Color.R, colorList[i].Pallet.Color.G, colorList[i].Pallet.Color.B, pixel.Color.A);
                if (colorList[i].Pallet.Pos < index)
                {
                    p1 = colorList[i].Pallet;
                    p2 = p1;
                }
                if (colorList[i].Pallet.Pos > index)
                {
                    p2 = colorList[i].Pallet;
                    break;
                }
            }

            if (index > colorList.Last().Pallet.Pos)
            {
                p1 = colorList.Last().Pallet;
                p2 = colorList.Last().Pallet;
            }

            int m = index - p1.Pos;
            int n = p2.Pos - index + 1;
            int size = m + n;

            //division
            R = (byte)Math.Round((double)(n * p1.Color.R + m * p2.Color.R) / size, 0);
            G = (byte)Math.Round((double)(n * p1.Color.G + m * p2.Color.G) / size, 0);
            B = (byte)Math.Round((double)(n * p1.Color.B + m * p2.Color.B) / size, 0);

            return new GSColor(R, G, B, pixel.Color.A);
        }

        private void GRDApply_Click(object sender, RoutedEventArgs e)
        {
            if (SourceImage == null)
                return;
            if (!colorList.Any())
                return;

            List<Pixel> pixels = new List<Pixel>();

            foreach (Pixel pixel in SourceImage.Pixels)
                pixels.Add(new Pixel(pixel.Pos, getGradationColor(pixel)));

            ResultImage = BitmapSource.Create(SourceImage.Width, SourceImage.Height, SourceImage.DpiX, SourceImage.DpiY, PixelFormats.Pbgra32, null, BMP.ExportPixel(pixels.ToArray(), SourceImage.Width, SourceImage.Height), SourceImage.Stride);

            image.Source = ResultImage;
        }

        private void ImgOpen_Click(object sender, RoutedEventArgs e)
        {
            string filename = FileDialog_Open("image files (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp");

            if (filename == null)
                return;

            SourceImage = new BMP(filename);

            image.Source = SourceImage.ExportSource();

            isSaved = false;
        }

        private void ImgSave_Click(object sender, RoutedEventArgs e)
        {
            isSaved = SaveImage();
        }

        private void ImgClose_Click(object sender, RoutedEventArgs e)
        {
            if (!isSaved)
            {
                MessageBoxResult result = CheckSave();

                if(result == MessageBoxResult.Yes)
                {
                    SaveImage();
                    image.Source = new BitmapImage();
                    SourceImage = null;
                }

                if (result == MessageBoxResult.No)
                {
                    image.Source = new BitmapImage();
                    SourceImage = null;
                }

                if(result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (!isSaved)
            {
                MessageBoxResult result = CheckSave();

                if (result == MessageBoxResult.Yes)
                {
                    SaveImage();
                }

                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            Application.Current.Shutdown();
        }

        private bool SaveImage()
        {
            if (ResultImage == null) return false;

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "PNG file(*.png)|*.png";

            if (dialog.ShowDialog() == true)
            {
                using (FileStream filestream = new FileStream(dialog.FileName, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();

                    encoder.Interlace = PngInterlaceOption.On;

                    encoder.Frames.Add(BitmapFrame.Create(ResultImage));
                    encoder.Save(filestream);
                }

                return true;
            }

            return false;
        }

        private MessageBoxResult CheckSave()
        {
            return MessageBox.Show("Save image before closing?", "alert", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes);
        }

        private string FileDialog_Open(string filter)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.Filter = filter;

            if (fileDialog.ShowDialog() == false)
                return null;

            return fileDialog.FileName;
        }

        private string FileDialog_Save(string filter)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();

            fileDialog.Filter = filter;

            if (fileDialog.ShowDialog() == false)
                return null;

            return fileDialog.FileName;
        }

        private void SliderR_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;

            try
            {
                LabelR.Content = (byte)slider.Value;

                pallet.Color.R = (byte)slider.Value;

                Preview.Background = new SolidColorBrush(Color.FromRgb(pallet.Color.R, pallet.Color.G, pallet.Color.B));
            }
            catch { }
        }
        private void SliderG_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;

            try
            {
                LabelG.Content = (byte)slider.Value;

                pallet.Color.G = (byte)slider.Value;

                Preview.Background = new SolidColorBrush(Color.FromRgb(pallet.Color.R, pallet.Color.G, pallet.Color.B));
            }
            catch { }
        }
        private void SliderB_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;

            try
            {
                LabelB.Content = (byte)slider.Value;

                pallet.Color.B = (byte)slider.Value;

                Preview.Background = new SolidColorBrush(Color.FromRgb(pallet.Color.R, pallet.Color.G, pallet.Color.B));
            }
            catch { }
        }
    }
}
