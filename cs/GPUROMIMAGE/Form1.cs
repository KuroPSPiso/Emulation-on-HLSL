using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GPUROMIMAGE
{
    public partial class Form1 : Form
    {
        int resolution = 1024;
        string filename = "untitled";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                byte[] data = File.ReadAllBytes(ofd.FileName);
                filename = Path.GetFileName(ofd.FileName);

                createImage(data);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setResolutionValue(trackBar1.Value);
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            setResolutionValue(trackBar1.Value);
        }

        private int setResolutionValue(int newValue)
        {
            resolution = 32;
            for (int i = 0; i < newValue; i++)
            {
                resolution *= 2;
            }

            lblResolution.Text = $"{resolution}x{resolution}";

            return resolution;
        }

        private void createImage(byte[] buffer)
        {
            int len = buffer.Length;

            var imgData = new Bitmap(resolution, resolution);
            for (int x = 0; x < imgData.Width; x++)
            {
                for (int y = 0; y < imgData.Height; y++)
                {
                    imgData.SetPixel(x, y, Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
                }
            }

            //res-ypos (invert) required for hlsl texture pos where (0,0) starts bottom left
            imgData.SetPixel(0, resolution - 1 - 0, Color.FromArgb(0xFF, 0x00, 0x00, (len >> 24) & 0xFF));
            imgData.SetPixel(1, resolution - 1 - 0, Color.FromArgb(0xFF, (len >> 16) & 0xFF, (len >> 8) & 0xFF, len & 0xFF));

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
                    //res-px/res (invert)
                    imgData.SetPixel(px % resolution, resolution - 1  - px / resolution, Color.FromArgb(0xFF, colorChannel[0], colorChannel[1], colorChannel[2]));
                    colorChannel = new int[]{
                        0xFF, 0x00, 0x00, 0x00
                    };

                    px++;
                    xcc = 0;
                }
            }
            if (xcc > 0)
            {
                //res-px/res (invert)
                imgData.SetPixel(px % resolution, resolution - 1 - px / resolution, Color.FromArgb(0xFF, colorChannel[0], colorChannel[1], colorChannel[2]));
                colorChannel = new int[]{
                    0xFF, 0x00, 0x00, 0x00
                };

                px++;
                xcc = 0;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = filename;
            sfd.DefaultExt = "png";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                imgData.Save(sfd.FileName, ImageFormat.Png);
            }

            pictPreview.Image = imgData;

        }
    }
}
