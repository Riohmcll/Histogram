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
using OpenCvSharp;


namespace Histogram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : System.Windows.Window
    {
        void CreateHist(out Mat HistogramMat, Mat HisResult, float max, out List<float> Columns)
        {
            HistogramMat = new Mat(100, 180, MatType.CV_8UC3);
            Vec3b cyan = new Vec3b(byte.MaxValue, byte.MaxValue, 0);
            Vec3b red = new Vec3b(0, 0, byte.MaxValue);
            Columns = new List<float>();
            for (int i = 0; i < 180; i++)
            {
                float column = HisResult.Get<float>((int)Math.Floor(i / 10.0), 0);
                if (i % 10 == 0)
                {
                    Columns.Add(column);
                }
                float columnPercent = column / max;
                for (int j = 0; j < 100; j++)
                {
                    if ((100 - j) < columnPercent * 100)
                    {
                        HistogramMat.At<Vec3b>(j, i) = cyan;
                    }
                    else
                    {
                        HistogramMat.At<Vec3b>(j, i) = red;
                    }
                }
            }
        }

        void CreateHist(out Mat HistogramMat, int[] Columns, float max)
        {
            HistogramMat = new Mat(100, 180 - exValueBias, MatType.CV_8UC3);
            Vec3b cyan = new Vec3b(byte.MaxValue, byte.MaxValue, 0);
            Vec3b red = new Vec3b(0, 0, byte.MaxValue);

            for (int i = 0; i < 180 - exValueBias; i++)
            {
                float columnPercent = Columns[i] / max;
                for (int j = 0; j < 100; j++)
                {
                    if ((100 - j) < columnPercent * 100)
                    {
                        HistogramMat.At<Vec3b>(j, i) = cyan;
                    }
                    else
                    {
                        HistogramMat.At<Vec3b>(j, i) = red;
                    }
                }
            }
        }

        private void ShowImageAndHist(String file)
        {
            refImage = new Mat(file);
            ShowImageAndHist();
        }

        private void ShowMat(Image location, Mat mat)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = mat.ToMemoryStream();
            bitmapImage.EndInit();
            location.Source = bitmapImage;
        }

        void CalculateSatHis(Mat HSVImage, out int[] Hues, double threshHoldS, double threshHoldV)
        {
            Hues = new int[180];
            if (HSVImage == null)
                return;
            for (int i = 0; i < Hues.Length; i++)
            {
                Hues[i] = 0;
            }
            for (int i = 0; i < HSVImage.Height; i++)
            {
                for (int j = 0; j < HSVImage.Width; j++)
                {
                    Vec3b color = HSVImage.At<Vec3b>(i, j);
                    if (color.Item1 < threshHoldS && color.Item2 < threshHoldV)
                    {
                        if (color.Item0 > 179)
                        {
                            continue;
                        }
                        else
                        {
                            Hues[color.Item0]++;
                        }
                    }
                }
            }
            Array.Sort<int>(Hues);
            double sum = 0;
            int n = 0;

            for (int i = 0; i < Hues.Length - exValueBias; i++)
            {
                sum += Hues[i] * i;
                n += Hues[i];

            }
            double avg = sum / n;

            double sqrdiff = 0;


            for (int i = 0; i < Hues.Length - exValueBias; i++)
            {
                var diff = i - avg;
                sqrdiff += diff * diff * Hues[i];
            }
            sqrdiff /= avg;

            double stderr = Math.Sqrt(sqrdiff / n);
            double stddiv = Math.Sqrt(sqrdiff);
            Stat.Content = $"avg:{avg}, stderr:{stderr},\n stddiv:{stddiv}";
        }

        Mat HSVImage;
        Mat refImage;
        int exValueBias = 1;
        private void ShowImageAndHist()
        {

            HSVImage = refImage.CvtColor(ColorConversionCodes.BGR2HSV);

            Mat HisResult = new Mat();
            Cv2.CalcHist(new Mat[] { HSVImage }, new int[] { 0 }, null, HisResult, 1, new int[] { 18 }, new Rangef[] { new Rangef(0, 180) });

            Mat SatuatedImage = new Mat();
            Cv2.ColorChange(HSVImage, null, SatuatedImage, 1, 4, 1);

            CalculateSatHis(HSVImage, out var Hues, Satvalue.Value, ValValue.Value);



            CreateHist(out var HistogramMat, Hues, Hues[Hues.Length-1]);

            ShowMat(RefImage, refImage);

            ShowMat(HistogramImage, HistogramMat);

        }

        public MainWindow()
        {
            InitializeComponent();
            ShowImageAndHist(@"C:\Users\huo\Desktop\portrait.jpg");
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
            case Key.Space:
                ShowImageAndHist(@"C:\Users\huo\Desktop\aigen.jpg");
                break;
            case Key.K:
                ShowImageAndHist(@"C:\Users\huo\Desktop\portrait.jpg");
                break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var fildDialog = new OpenFileDialog();

            string FiLename;
            if (fildDialog.ShowDialog() == true)
            {

                FiLename = fildDialog.FileName;
                ShowImageAndHist(FiLename);
            }

        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (refImage == null) return;
            if (HSVImage == null) return;
            CalculateSatHis(HSVImage, out var Hues, Satvalue.Value, ValValue.Value);

            CreateHist(out var HistogramMat, Hues, (float)refImage.Height * refImage.Width / 100);


            ShowMat(HistogramImage, HistogramMat);
        }

    }
}
