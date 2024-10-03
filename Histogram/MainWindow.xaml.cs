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
        void CreateHist(Mat HistogramMat, Mat HisResult, float max, out List<float> Columns)
        {
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

        private void ShowImageAndHist(String file)
        {
            Mat refImage = new Mat(file);
            ShowImageAndHist(refImage);
        }
        private void ShowImageAndHist(Mat refImage)
        {

            Mat HSVImage = refImage.CvtColor(ColorConversionCodes.BGR2HSV);

            Mat HisResult = new Mat();
            Cv2.CalcHist(new Mat[] { HSVImage }, new int[] { 0 }, null, HisResult, 1, new int[] { 18 }, new Rangef[] { new Rangef(0, 180) });
            Mat SatuatedImage = new Mat();
            Cv2.ColorChange(HSVImage, null, SatuatedImage,1,4,1);
            
            Mat HistogramMat = new Mat(100, 180, MatType.CV_8UC3);

            CreateHist(HistogramMat, HisResult, (float)RefImage.Height * refImage.Width / 2, out var Columns);

            string combinedString = string.Join(", \n", Columns.ToArray());

            Hist.Content = combinedString;

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = HSVImage.ToMemoryStream();
            bitmapImage.EndInit();
            RefImage.Source = bitmapImage;

            BitmapImage bitmapHistImage = new BitmapImage();
            bitmapHistImage.BeginInit();
            bitmapHistImage.StreamSource = HistogramMat.ToMemoryStream();
            bitmapHistImage.EndInit();
            HistogramImage.Source = bitmapHistImage;

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
    }
}
