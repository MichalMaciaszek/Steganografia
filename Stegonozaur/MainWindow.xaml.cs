using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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


namespace Steganografia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage bitmap;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Title = "Open Image";
            ofd.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.bmp, *.png) | *.jpg; *.jpeg; *.jpe; *.bmp; *.png"; ;

            if (ofd.ShowDialog() == true)
            {
                bitmap = new BitmapImage(new Uri(ofd.FileName));
                ImageInButton.Source = bitmap;
                checkIfEncryptButtonEnable();
                charsToEncryptInImageTextBlock.Text = "Maksymalna ilosc znakow: " + (((bitmap.PixelHeight * bitmap.PixelWidth) / 4)-2);
            }   

        }
        static byte[] EncodeToBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        int howlong;
        WriteableBitmap im2;


        
        private void EncryptButton_Click(object sender, RoutedEventArgs e)
      
        {
            if (InputTextBox.Text != null || InputTextBox.Text != "")
            {
                howlong = InputTextBox.Text.Length * 16;
                //image to byte array
                WriteableBitmap im = new WriteableBitmap(bitmap);

                var width = im.PixelWidth;
                var height = im.PixelHeight;
                var stride = width * ((im.Format.BitsPerPixel) / 8);

                var bitmapData = new byte[height * stride];

                im.CopyPixels(bitmapData, stride, 0);
                String sx = InputTextBox.Text;
                InputTextBox.Text += "##";
             
                byte[] by = EncodeToBytes(InputTextBox.Text);
                InputTextBox.Text = sx;
                if (sx.Count() > ((bitmap.PixelHeight * bitmap.PixelWidth) / 4)) { return; }


                int k = 0;
                for (int j = 0; j < by.Length; j++)
                {
                    
                    for (int i = 7; i > -1; i--)
                    {
                        int bitNumber = i;
                        var bit = (by[j] & (1 << bitNumber)) != 0;

                        if (bit == false)
                        {
                            if (bitmapData[k] % 2 == 1)
                                bitmapData[k]--;
                        }
                        if (bit == true)
                        {
                            if (bitmapData[k] % 2 == 0)
                                bitmapData[k]++;
                        }
                        k++;
                    }
                    //}
                }

               

                PixelFormat pf = PixelFormats.Pbgra32;
               

                im2 = new WriteableBitmap(width, height, 100, 100, pf, new BitmapPalette(new List<Color> { Color.FromArgb(255, 255, 0, 0) }));
                im2.WritePixels(new Int32Rect(0, 0, width, height), bitmapData, stride, 0);
                ImageInButton.Source = im2;
                Console.WriteLine("zakonczono");
            }
            else
            {
                MessageBoxResult m = MessageBox.Show("You need to enter the message first!", "Alert!");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".png";
            sfd.Filter = "Pliki PNG|*.png";
            if (sfd.ShowDialog() == true)
            {
                if (sfd.FileName != string.Empty)
                {
                    using (FileStream stream5 = new FileStream(sfd.FileName, FileMode.Create))
                    {
                        PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                        encoder5.Frames.Add(BitmapFrame.Create(im2));
                        encoder5.Save(stream5);
                    }
                }
            }
        }
        private byte[] toByteArray(String text)
        {
            int byteIndex;
            int bitIndex;
            int numBytes;
            byte[] byt;
            numBytes = text.Length / 8;

            byt = new byte[numBytes];
            byteIndex = 0;
            bitIndex = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '1')
                {
                    byt[byteIndex] |= (byte)(1 << (7 - bitIndex));
                }

                bitIndex++;

                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }
            return byt;
        }

        static string DecodeToString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }




        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            {
               
                WriteableBitmap im = new WriteableBitmap(bitmap);

                var width = im.PixelWidth;
                var height = im.PixelHeight;
                var stride = width * ((im.Format.BitsPerPixel) / 8);

                var bitmapData = new byte[height * stride];

                im.CopyPixels(bitmapData, stride, 0);
                
                byte[] by;
                string b = null;

                if (bitmapData[0] % 2 == 0)
                    b += '0';
                else
                    b += '1';

                for (int j = 1; j < bitmapData.Length; j++)
                {
                    var regex = new Regex(@".*00100011000000000010001100000000$");
                    Match match = regex.Match(b);

                    if (!match.Success)
                    {
                        if (bitmapData[j] % 2 == 0)
                        {
                            b += '0';
                        
                        }
                        else
                        {
                            b += '1';
                           
                        }
                        if (match.Success)
                            break;
                    }
                }
                by = toByteArray(b);
                String s = DecodeToString(by);
                String ss = s.Substring(0, s.Count()-2);
                OutputTextBox.Text = ss;
            }
        }




        

        private void SaveOutputButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".txt";
            sfd.Filter = "Text documents (.txt)|*.txt";

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, OutputTextBox.Text, Encoding.GetEncoding("Windows-1250"));
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            InputTextBox.Clear();
            OutputTextBox.Clear();


        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkIfEncryptButtonEnable();
            charsToEncryptInMessageTextBlock.Text = "Ilość znaków: " + InputTextBox.Text.Length;
        }

        private void loadTextButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".txt";
            ofd.Filter = "Text documents (.txt)|*.txt";

            if (ofd.ShowDialog() == true)
            {
                InputTextBox.Text = File.ReadAllText(ofd.FileName, Encoding.GetEncoding("Windows-1250"));
            }
        }

        private void checkIfEncryptButtonEnable()
        {
            if (bitmap != null && InputTextBox.Text.Length > 0)
            {
                if (((bitmap.PixelHeight * bitmap.PixelWidth) / 4) -2> InputTextBox.Text.Length)
                {
                    EncryptButton.IsEnabled = true;
                }
                else EncryptButton.IsEnabled = false;
            }
            else EncryptButton.IsEnabled = false;

            if (bitmap != null) DecryptButton.IsEnabled = true; else DecryptButton.IsEnabled = false;
        }

        BitmapImage encryptMessageInImage()
        {

            return null;
        }

        private static byte[] ConvertToByteArray(string str, Encoding encoding)
        {
            return encoding.GetBytes(str);
        }

        public static String ToBinary(Byte[] data)
        {
            return string.Join("", data.Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
        }

        public static byte[] BinaryStringToByteArray(String input)
        {
            int numOfBytes = input.Length / 8;
            byte[] bytes = new byte[numOfBytes];
            for (int i = 0; i < numOfBytes; ++i)
            {
                bytes[i] = Convert.ToByte(input.Substring(8 * i, 8), 2);
            }

            return bytes;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}