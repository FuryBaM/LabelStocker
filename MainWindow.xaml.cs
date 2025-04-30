using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Linq;
using IronBarCode;
using HidLibrary;


namespace ProductStorage
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ProductDatabase database = new ProductDatabase();
        private Product selectedProduct = null;
        private string scannedData = "";
        public MainWindow()
        {
            InitializeComponent();
            ShowPrinters();
            database.OnProductCreate += OnProductCreate;
            //GetDevice();
            //SelectKeyboard(availableKeyboards[0]);
        }

        public void ResetDataGrid()
        {
            productDataGrid.Items.Clear();
            foreach (Product product in database.Products)
            {
                productDataGrid.Items.Add(product);
            }
        }
        private void Window_PreviewKeyDown(object sender,  KeyEventArgs e)
        {
            //Console.WriteLine(e.Key + " " + e.OriginalSource + " " + e.KeyStates);
            if (e.Key == Key.Enter)
            {
                ProcessScannedData();
            }
            else
            {
                if (e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    KeyConverter converter = new KeyConverter();
                    string number = converter.ConvertToString(e.Key);
                    
                    debugLabel.Content += number;
                    scannedData += number;
                }
            }
        }

        private void ProcessScannedData()
        {
            MessageBox.Show(scannedData);
            scannedData = "";
            debugLabel.Content = "";
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddProductDialog addProductDialog = new AddProductDialog(database);
            if (addProductDialog.ShowDialog() == true)
                if (addProductDialog.GetResult() != null)
                {
                    MessageBox.Show($"Товар {addProductDialog.GetResult().name} создан");
                }
        }

        private void OnProductCreate(Product product)
        {
            productDataGrid.Items.Add(product);
        }

        private void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            ManageGroupsDialog addProductDialog = new ManageGroupsDialog(database);
            addProductDialog.ShowDialog();
        }

        private Bitmap CreateLabelImage(Product product)
        {
            int width = 29*24;
            int height = 20*24;
            int x = 20;
            int y = 20;

            Bitmap labelImage = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(labelImage))
            {
                Font nameFont = new Font("Codabar", 16, System.Drawing.FontStyle.Regular, GraphicsUnit.Millimeter);
                Font priceFont = new Font("Codabar", 16, System.Drawing.FontStyle.Regular, GraphicsUnit.Millimeter);
                Font codeFont = new Font("Codabar", 18, System.Drawing.FontStyle.Regular, GraphicsUnit.Millimeter);

                StringFormat textFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.MeasureTrailingSpaces,
                    Trimming = StringTrimming.Character
                };
                RectangleF nameRect = new RectangleF(x, 0, width - x, 200 - y);
                graphics.DrawString(s: product.name, font: nameFont, brush: System.Drawing.Brushes.Black, layoutRectangle: nameRect, format: textFormat);
                RectangleF priceRect = new RectangleF(x, y + 360, width - x, 100 - y);
                graphics.DrawString(s: $"Цена {product.price}", font: priceFont, brush: System.Drawing.Brushes.Black, layoutRectangle: priceRect, format: textFormat);
                RectangleF codeRect = new RectangleF(x, y + 270, width - x, 100 - y);
                //graphics.FillRectangle(System.Drawing.Brushes.White, codeRect);
                var myBarcode = BarcodeWriter.CreateBarcode(CalculateEan13("2" + "00000" + product.id.ToString("000000")), BarcodeWriterEncoding.EAN13, 600, 120);
                System.Drawing.Image image = myBarcode.Image;
                graphics.DrawImage(image, (width - image.Width) / 2, 160);
                graphics.DrawString(s: CalculateEan13("2" + "00000" + product.id.ToString("000000")), font: codeFont, brush: System.Drawing.Brushes.Black, layoutRectangle: codeRect, format: textFormat);
            }
            return labelImage;
        }

        private void ShowPrinters()
        {
            PrinterSettings.StringCollection printers = PrinterSettings.InstalledPrinters;
            foreach (string printer in printers)
            {
                printersComboBox.Items.Add(new ComboBoxItem() { Content = printer });
            }
            if (!printersComboBox.Items.IsEmpty)
            {
                printersComboBox.SelectedIndex = 0;
            }
        }

        private void PrintPriceTag()
        {
            Product product = selectedProduct;
            if (product != null)
            {
                Bitmap bitmap = CreateLabelImage(product);
                PrintDocument pd = new PrintDocument();
                pd.PrinterSettings.PrinterName = printersComboBox.Text;
                pd.PrintPage += (sender, e) =>
                {
                    e.Graphics.DrawImage(bitmap, e.PageBounds);
                };
                pd.Print();
            }
            else
            {
                MessageBox.Show("Выберите товар из списка.");
            }
        }

        private void PrintTagButton_Click(object sender, RoutedEventArgs e)
        {
            PrintPriceTag();
        }

        private void ShowTagButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProduct != null)
            {
                var handle = CreateLabelImage(selectedProduct).GetHbitmap();
                ImageSource imageSource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                image.Source = imageSource;
            }
            else
            {
                MessageBox.Show("Выберите товар из списка.");
            }
        }

        private void ProductDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (productDataGrid.SelectedIndex != -1)
            {
                Product product = (Product)productDataGrid.SelectedItem;
                selectedProduct = product;
            }
            else
            {
                selectedProduct = null;
            }
        }

        public static string CalculateEan13(string value)
        {
            string temp = value;
            int sum = 0;
            int digit = 0;

            // Calculate the checksum digit here.
            for (int i = temp.Length; i >= 1; i--)
            {
                digit = Convert.ToInt32(temp.Substring(i - 1, 1));
                // This appears to be backwards but the 
                // EAN-13 checksum must be calculated
                // this way to be compatible with UPC-A.
                if (i % 2 == 0)
                { // odd  
                    sum += digit * 3;
                }
                else
                { // even
                    sum += digit * 1;
                }
            }
            int checkSum = (10 - (sum % 10)) % 10;
            return $"{temp}{checkSum}";
        }

        private void SearchProductButton_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(searchProductButton.Text))
            {
                productDataGrid.Items.Clear();
                Regex regex = new Regex($@"{searchProductButton.Text}", RegexOptions.IgnoreCase);
                foreach (Product product in database.Products)
                {
                    if (regex.IsMatch(product.name))
                    {
                        productDataGrid.Items.Add(product);
                    }
                }
            }
            else
            {
                ResetDataGrid();
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            database.Save();
        }
        List<HidDevice> availableKeyboards = new List<HidDevice>();
        private HidDevice selectedKeyboard;
        private void GetDevice()
        {
            HidDevice[] devices = HidDevices.Enumerate().ToArray();
            foreach (HidDevice device in devices)
            {
                if (device.Capabilities.UsagePage == 0x01 && device.Capabilities.Usage == 0x06)
                {
                    Console.WriteLine(device.Description);
                    availableKeyboards.Add(device);
                }
            }
        }
        private void SelectKeyboard(HidDevice device)
        {
            selectedKeyboard = device;
            selectedKeyboard.OpenDevice();
            selectedKeyboard.MonitorDeviceEvents = true;
            selectedKeyboard.Inserted += SelectedKeyboard_Inserted;
            selectedKeyboard.Removed += SelectedKeyboard_Removed;
            selectedKeyboard.ReadReport(OnReport);
        }

        private void SelectedKeyboard_Removed()
        {
            Console.WriteLine("Device disconnected");
        }

        private void SelectedKeyboard_Inserted()
        {
            Console.WriteLine("Device connected");
        }

        private void OnReport(HidReport report)
        {
            byte data = report.Data[0];
            if (data != 0)
            {
                //Console.WriteLine("Keyboard event " + BitConverter.ToString(data));
                Console.WriteLine("recv: {0}\n{0}", string.Join(", ", report.Data.Select(b=>b.ToString("X2"))), data);
                selectedKeyboard.ReadReport(OnReport);
            }
        }

        private bool isEmptyData(byte[] data)
        {
            bool isEmpty = true;
            foreach(byte bit in data)
            {
                if (bit != 00)
                {
                    isEmpty = false;
                }
            }
            return isEmpty;
        }
    }
}
