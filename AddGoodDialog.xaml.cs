using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProductStorage
{
    /// <summary>
    /// Логика взаимодействия для AddGoodDialog.xaml
    /// </summary>
    public partial class AddProductDialog : Window
    {
        private ProductDatabase _database;
        private Product _product;
        public AddProductDialog(ProductDatabase database)
        {
            InitializeComponent();
            _database = database;
            var groups = database.GetGroups();
            foreach (ProductGroup group in groups) {
                var item = new ComboBoxItem();
                item.Content = group.name;
                comboBox.Items.Add(item);
            }
            comboBox.SelectedIndex = 0;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            priceTextBox.SelectAll();
            priceTextBox.Focus();
        }

        public Product GetResult()
        {
            return _product;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            bool canCreate = true;
            if (string.IsNullOrWhiteSpace(priceTextBox.Text))
            {
                priceTextBox.Text = "0";
                canCreate = false;
            }
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Нужно задать имя товара!");
                canCreate = false;
            }
            if (canCreate == true)
            {
                _product = new Product { name = nameTextBox.Text, price = Convert.ToUInt32(priceTextBox.Text), groupName = _database.GetGroupWithName("Прочее").name };
                _database.AddProduct(_product);
                _database.AddToGroup(_database.GetGroupWithName(comboBox.Text), _product);
                DialogResult = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
