using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

using Color = System.Drawing.Color;

namespace romToImg
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int resolution = 1024;
        string filename = "untitled";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                byte[] data = File.ReadAllBytes(ofd.FileName);

                createImage(data);
            }
        }

        private int setResolutionValue(int newValue)
        {
            resolution = 32;
            for(int i = 0; i < newValue; i++)
            {
                resolution *= 2;
            }

            lblResolution.Content = $"{resolution}x{resolution}";

            return resolution;
        }

        private void createImage(byte[] buffer)
        {
            int len = buffer.Length;

            var imgData = new Bitmap(resolution, resolution);
            for(int x = 0; x < imgData.Width; x++)
            {
                for(int y = 0; x < imgData.Height; y++)
                {
                    imgData.SetPixel(x, y, Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
                }
            }

            imgData.SetPixel(0, 0, Color.FromArgb(0xFF, 0x00, 0x00, (len >> 24) & 0xFF));
            imgData.SetPixel(1, 0, Color.FromArgb(0xFF, (len >> 16) & 0xFF, (len >> 8) & 0xFF, len & 0xFF));

            int[] colorChannel = new int[]{
                0xFF, 0x00, 0x00, 0x00
            };

            int xcc = 0;
            int px = 2;
            for (int i = 0; i < len; i++)
            {
                colorChannel[xcc] = buffer[i];
                xcc++;
                if (xcc >= 3)
                {
                    colorChannel = new int[]{
                        0xFF, 0x00, 0x00, 0x00
                    };

                    imgData.SetPixel(px % resolution, px / resolution, Color.FromArgb(0xFF, colorChannel[0], colorChannel[1], colorChannel[2]));

                    px++;
                    xcc = 0;
                }
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = filename;
            sfd.DefaultExt = "png";
            if(sfd.ShowDialog() == true)
            {
                imgData.Save(sfd.FileName, ImageFormat.Png);
            }

        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            setResolutionValue((int)e.NewValue);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            setResolutionValue((int)slider.Value);
        }
    }
}
