using Svg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace IconToolMigrationForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static string ToBase64String(Image bmp)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, bmp);
            stream.Position = 0;

            byte[] memBytes = new byte[stream.Length];
            stream.Read(memBytes, 0, (int)stream.Length);
            stream.Close();

            return Convert.ToBase64String(memBytes);
        }

        public static Image DeserializeFromBase64Text(string text)
        {
            Image img = null;
            byte[] memBytes = Convert.FromBase64String(text);
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(memBytes);
            img = (Image)formatter.Deserialize(stream);
            stream.Close();
            return img;
        }

        private static System.Drawing.Image ResizeImage(System.Drawing.Image image, int width, int height)
        {
            //a holder for the result
            Bitmap result = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
            // set the resolutions the same to avoid cropping due to resolution differences
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //use a graphics object to draw the resized image into the bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap
            return result;
        }



        private void button2_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(txtIconPath.Text))
            {
                MessageBox.Show("Icon folder path is required.", "Icon Tool", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (String.IsNullOrEmpty(txtIVRPath.Text))
            {
                MessageBox.Show("IVR folder path is required.", "Icon Tool", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //DirectoryInfo d = new DirectoryInfo(txtPath.Text);
            //FileInfo[] Files = d.GetFiles("*.bmp");


            /*var iconFilePaths = Directory.GetFiles(txtIconPath.Text, "*.bmp", SearchOption.AllDirectories);
            string IVRPath = txtIVRPath.Text;
            string result;

            foreach (var iconFilePath in iconFilePaths)
            {
                using (var bmp = (Bitmap)Image.FromFile(iconFilePath))
                {
                    Bitmap _buffer;
                    if (bmp.Width != 32 || bmp.Height != 32)
                        _buffer = ResizeImage(bmp, 32, 32) as Bitmap;
                    else
                        _buffer = bmp;

                    result = ToBase64String(_buffer);
                    string IVRFullPath = IVRPath + "\\" + Path.GetFileNameWithoutExtension(iconFilePath) + ".xml";

                    XmlDocument doc = new XmlDocument();
                    doc.Load(IVRFullPath);
                    
                    XmlNode aNodes = doc.SelectSingleNode("/GlobalLibraryItem/Icon");
                    aNodes.InnerText = result;
                    System.IO.File.SetAttributes(IVRFullPath, System.IO.FileAttributes.Normal);
                    doc.Save(IVRFullPath);
                }
            }*/
            var iconFilePaths = Directory.GetFiles(txtIconPath.Text, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".bmp") || s.EndsWith(".svg"));

            foreach (var iconFilePath in iconFilePaths) {
                var currentPath = iconFilePath;
                if (iconFilePath.Contains(".bmp")) {
                    handleBMPFiles(iconFilePath);
                } else
                if (iconFilePath.Contains(".svg")) {
                    handleSVGFiles(iconFilePath);

                }
            }

                MessageBox.Show("MIGRATION HAS SUCCESSFULLY COMPLETED", "Icon Tool", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void handleBMPFiles(string pathIcon) {
            string IVRPath = txtIVRPath.Text;
            string result;
            using (var bmp = (Bitmap)Image.FromFile(pathIcon))
            {
                Bitmap _buffer;
                if (bmp.Width != 32 || bmp.Height != 32)
                    _buffer = ResizeImage(bmp, 32, 32) as Bitmap;
                else
                    _buffer = bmp;

                result = ToBase64String(_buffer);
                string IVRFullPath = IVRPath + "\\" + Path.GetFileNameWithoutExtension(pathIcon) + ".xml";

                XmlDocument doc = new XmlDocument();
                doc.Load(IVRFullPath);

                XmlNode aNodes = doc.SelectSingleNode("/GlobalLibraryItem/Icon");
                aNodes.InnerText = result;
                System.IO.File.SetAttributes(IVRFullPath, System.IO.FileAttributes.Normal);
                doc.Save(IVRFullPath);
            }
        }
        private void handleSVGFiles(string pathIcon) {
            string IVRPath = txtIVRPath.Text;
            string result;
            var byteArray = Encoding.ASCII.GetBytes(pathIcon);

            using (var stream = new MemoryStream(byteArray))
            {
                var svgDocument = SvgDocument.Open(pathIcon);
                var bitmap = svgDocument.Draw();
                Bitmap _buffer;
                if (bitmap.Width != 32 || bitmap.Height != 32)
                    _buffer = ResizeImage(bitmap, 32, 32) as Bitmap;
                else
                    _buffer = bitmap;

                result = ToBase64String(_buffer);
                string IVRFullPath = IVRPath + "\\" + Path.GetFileNameWithoutExtension(pathIcon) + ".xml";

                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(IVRFullPath);
                }
                catch (Exception ex)
                {
                    txtReport.Text += "[SVG] Cannot find: " + IVRFullPath + "\r\n";
                    return;
                }
                

                XmlNode aNodes = doc.SelectSingleNode("/GlobalLibraryItem/Icon");
                aNodes.InnerText = result;
                System.IO.File.SetAttributes(IVRFullPath, System.IO.FileAttributes.Normal);
                doc.Save(IVRFullPath);

                txtReport.Text += "[SVG] Icon successfully loaded: " + IVRFullPath + "\r\n";

            }
        }

        public void ChooseFolder()
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtIconPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ChooseFolder();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtIVRPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
