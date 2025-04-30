using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using CsvHelper.Configuration;
using System.Globalization;

namespace ProductStorage
{
    public class ProductDatabase
    {
        private List<ProductGroup> _productGroups = new List<ProductGroup>();
        private List<Product> _products = new List<Product>();
        public List<Product> Products { get { return _products; } private set { } }
        public delegate void ProductCreation(Product product);
        public event ProductCreation OnProductCreate;
        public event ProductCreation OnProductRemove;

        public static string defaultGroup = "Прочее";

        public ProductDatabase()
        {
            Load();
            if (_productGroups.Where(x=>x.name == defaultGroup).Count() == 0)
            {
                AddGroup(new ProductGroup { name = defaultGroup, products = new List<Product>(), id = 0 });
            }
        }

        public List<ProductGroup> GetGroups() => _productGroups;

        public bool AddGroup(ProductGroup productGroup)
        {
            int maxId = 0;
            if (_productGroups.Count > 0)
                maxId = _productGroups.Max(x => x.id);
            if (_productGroups.Where(x => x.name.ToUpper() == productGroup.name.ToUpper()).Count() != 0)
            {
                MessageBox.Show("Такая группа уже есть в списке.");
                return false;
            }
            else
            {
                _productGroups.Add(productGroup);
                productGroup.id = maxId + 1;
                return true;
            }
        }

        public bool RemoveGroup(ProductGroup group)
        {
            if (group.name == defaultGroup)
            {
                MessageBox.Show("Нельзя удалять данную группу.");
                return false;
            }
            if (_productGroups.Contains(group))
            {
                _productGroups.Remove(group);
                return true;
            }
            return false;
        }

        public void AddProduct(Product product)
        {
            int maxId = 0;
            if (_products.Count > 0)
                maxId = Convert.ToInt32(_products.Max(x => x.id));
            if (_products.Select(x=>x.id == product.id).Count() != 0)
            {
                product.id = Convert.ToUInt32(maxId) + 1;
            }
            product.groupName = GetGroupWithName(defaultGroup).name;
            _products.Add(product);
            OnProductCreate?.Invoke(product);
        }

        public bool RemoveProduct(Product product)
        {
            if (_products.Contains(product))
            {
                _products.Remove(product);
                OnProductRemove?.Invoke(product);
                return true;
            }
            return false;
        }

        public bool AddToGroup(ProductGroup productGroup, Product product)
        {
            if (!productGroup.products.Contains(product))
            {
                var currentProductGroup = GetGroupWithName(product.groupName);
                if (currentProductGroup != null)
                {
                    RemoveFromGroup(currentProductGroup, product);
                }
                product.groupName = productGroup.name;
                productGroup.products.Add(product);
                return true;
            }
            return false;
        }


        public bool RemoveFromGroup(ProductGroup productGroup, Product product)
        {
            if (productGroup.products.Contains(product))
            {
                productGroup.products.Remove(product);
                product.groupName = GetGroupWithName(defaultGroup).name;
                return true;
            }
            return false;
        }

        public ProductGroup GetGroupWithName(string name)
        {
            try
            {
                return _productGroups.Single(x => x.name == name);
            }
            catch
            {
                return null;
            }
        }

        public Product GetProductByName(string name)
        {
            try
            {
                return _products.Single(x => x.name == name);
            }
            catch
            {
                return null;
            }
        }

        public void Save()
        {
            string groupPath = AppDomain.CurrentDomain.BaseDirectory + "groups.csv";
            using (StreamWriter streamWriter = new StreamWriter(groupPath))
            {
                using (CsvWriter csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    csvWriter.WriteRecords(_productGroups);
                }
            }
            string productsFilePath = AppDomain.CurrentDomain.BaseDirectory + "products.csv";
            using (StreamWriter streamWriter = new StreamWriter(productsFilePath))
            {
                using (CsvWriter csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    csvWriter.WriteRecords(_products);
                }
            }
            Console.WriteLine("Data saved");
        }

        public bool Load()
        {
            try
            {
                string groupPath = AppDomain.CurrentDomain.BaseDirectory + "groups.csv";
                string productsFilePath = AppDomain.CurrentDomain.BaseDirectory + "products.csv";

                if (!File.Exists(groupPath) || !File.Exists(productsFilePath)) return false;

                List<ProductGroup> groups;
                using (StreamReader streamReader = new StreamReader(groupPath))
                {
                    using (CsvReader csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        groups = csvReader.GetRecords<ProductGroup>().ToList();
                    }
                }

                List<Product> products;
                using (StreamReader streamReader = new StreamReader(productsFilePath))
                {
                    using (CsvReader csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        products = csvReader.GetRecords<Product>().ToList();
                    }
                }
                for (int i = 0; i < groups.Count; i++)
                {
                    groups[i].products = new List<Product>();
                    Console.WriteLine(groups[i]);
                }
                _productGroups = groups;
                for (int i = 0; i < products.Count; i++)
                {
                    Console.WriteLine(products[i].groupName);
                }
                _products = products;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return true;
        }
    }
}
