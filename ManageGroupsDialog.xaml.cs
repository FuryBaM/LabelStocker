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
using System.Windows.Shapes;

namespace GoodStorage
{
    /// <summary>
    /// Логика взаимодействия для ManageGroupsDialog.xaml
    /// </summary>
    public partial class ManageGroupsDialog : Window
    {
        private GoodDatabase _database;
        public ManageGroupsDialog(GoodDatabase database)
        {
            InitializeComponent();
            _database = database;
            var groups = database.GetGroups();
            foreach (GoodGroup group in groups)
            {
                var item = new ComboBoxItem();
                item.Content = group.name;
                comboBox.Items.Add(item);
            }
            comboBox.SelectedIndex = 0;
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(groupNameTextBox.Text))
            {
                GoodGroup goodGroup = new GoodGroup { name = groupNameTextBox.Text, goods = new List<Good>(), id = 0 };
                if (_database.AddGroup(goodGroup))
                {
                    var item = new ComboBoxItem();
                    item.Content = goodGroup.name;
                    comboBox.Items.Add(item);
                }
            }
        }

        private void RemoveGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (_database.RemoveGroup(_database.GetGroupWithName(comboBox.SelectionBoxItem.ToString())))
            {
                comboBox.Items.RemoveAt(comboBox.SelectedIndex);
                comboBox.SelectedIndex = 0;
            }
        }
    }
}
