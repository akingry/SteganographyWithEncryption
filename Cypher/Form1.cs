using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Cypher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Bitmap bitmap;
        Bitmap encodedImage;        

        private void Form1_Load(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog1.Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg";
            openFileDialog1.FilterIndex = 1;

            if (pictureBox1.Image == null)
            {
                button1.Enabled = false;
                button2.Enabled = false;
            }            
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (richTextBox1.Text == "") button1.Enabled = false;
            else button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text != "" && !string.IsNullOrEmpty(textBox1.Text))
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(richTextBox1.Text);
                int maxBytes = ((bitmap.Width * bitmap.Height) - 32) / 8;
                                
                if (messageBytes.Length > maxBytes - 4)
                {
                    throw new InvalidOperationException($"Message is too long to encode in this image. Max bytes: {maxBytes - 4}");
                }
                encodedImage = ImageSteganography.EncodeMessage(bitmap, richTextBox1.Text, textBox1.Text);
                pictureBox2.Image = encodedImage;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                richTextBox1.Clear();
                string message = ImageSteganography.DecodeMessage(encodedImage, textBox1.Text);
                richTextBox1.Text = message;
            }
        }

        private void loadRawImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = null;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                bitmap = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = bitmap;
                if (richTextBox1.Text == "")
                {
                    button1.Enabled = false;
                    button2.Enabled = false;
                }
                else
                {
                    button1.Enabled = true;
                    button2.Enabled = false;
                }
            }
        }

        private void loadEncodedImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            richTextBox1.Clear();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                encodedImage = new Bitmap(openFileDialog1.FileName);
                pictureBox2.Image = encodedImage;
                button1.Enabled = false;
                button2.Enabled = true;
            }
        }

        private void saveEncodedImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "outputImage.png");
                encodedImage.Save(filePath, ImageFormat.Png);
                MessageBox.Show("Saved " + filePath);
            }
        }
    }
    public static class ImageSteganography
    {
        public static string VigenereEncrypt(string text, string keyword)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                char keyChar = keyword[i % keyword.Length];
                char encryptedChar = (char)((c + keyChar) % 256);
                result.Append(encryptedChar);
            }
            return result.ToString();
        }

        public static string VigenereDecrypt(string text, string keyword)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                char keyChar = keyword[i % keyword.Length];
                char decryptedChar = (char)((c - keyChar + 256) % 256);
                result.Append(decryptedChar);
            }
            return result.ToString();
        }

        public static Bitmap EncodeMessage(Bitmap image, string message, string key)
        {
            string encryptedMessage = VigenereEncrypt(message, key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(encryptedMessage);
            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

            for (int i = 0; i < 32; i++)
            {
                int x = i % image.Width;
                int y = i / image.Width;

                Color pixel = image.GetPixel(x, y);

                int byteIndex = i / 8;
                int bitIndex = i % 8;

                int bit = (lengthBytes[byteIndex] >> bitIndex) & 1;
                int newPixelValue = (pixel.ToArgb() & ~1) | bit;

                image.SetPixel(x, y, Color.FromArgb(newPixelValue));
            }

            for (int i = 32; i < (messageBytes.Length * 8) + 32; i++)
            {
                int x = i % image.Width;
                int y = i / image.Width;

                Color pixel = image.GetPixel(x, y);

                int byteIndex = (i - 32) / 8;
                int bitIndex = (i - 32) % 8;

                int bit = (messageBytes[byteIndex] >> bitIndex) & 1;
                int newPixelValue = (pixel.ToArgb() & ~1) | bit;

                image.SetPixel(x, y, Color.FromArgb(newPixelValue));
            }

            return image;
        }


        public static string DecodeMessage(Bitmap image, string key)
        {
            byte[] lengthBytes = new byte[4];
            int bitCount = 0;

            for (int i = 0; i < 32; i++)
            {
                int x = i % image.Width;
                int y = i / image.Width;

                int pixelValue = image.GetPixel(x, y).ToArgb();
                int bit = pixelValue & 1;

                int byteIndex = bitCount / 8;
                int bitIndex = bitCount % 8;

                lengthBytes[byteIndex] |= (byte)(bit << bitIndex);

                bitCount++;
            }

            int messageLength = BitConverter.ToInt32(lengthBytes, 0);

            if (messageLength <= 0)
            {
                return "Invalid Message Length";
            }

            byte[] messageBytes = new byte[messageLength];
            bitCount = 0;

            for (int i = 32; i < 32 + (messageLength * 8); i++)
            {
                int x = i % image.Width;
                int y = i / image.Width;

                int pixelValue = image.GetPixel(x, y).ToArgb();
                int bit = pixelValue & 1;

                int byteIndex = bitCount / 8;
                int bitIndex = bitCount % 8;

                messageBytes[byteIndex] |= (byte)(bit << bitIndex);

                bitCount++;
            }

            string encryptedMessage = Encoding.UTF8.GetString(messageBytes);
            string decryptedMessage = VigenereDecrypt(encryptedMessage, key);

            return decryptedMessage;
        }


    }

}
