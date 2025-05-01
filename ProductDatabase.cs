using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using CsvHelper.Configuration;
using System.Globalization;

namespace LabelStocker
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
            if (_productGroups.Where(x=>x.name == defaultGroup).Count() == 0)
            {
                CreateGroup(new ProductGroup { name = defaultGroup, products = new List<Product>(), id = 0 });
            }
        }

        public List<ProductGroup> GetGroups() => _productGroups;

        public bool CreateGroup(ProductGroup productGroup)
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

        public void CreateProduct(Product product)
        {
            int maxId = 0;
            if (_products.Count > 0)
                maxId = Convert.ToInt32(_products.Max(x => x.id));
            if (_products.Select(x=>x.id == product.id).Count() != 0)
            {
                product.id = Convert.ToUInt32(maxId) + 1;
            }
            product.groupName = FindGroupByName(defaultGroup).name;
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

        public bool AddProductToGroup(ProductGroup productGroup, Product product)
        {
            if (!productGroup.products.Contains(product))
            {
                var currentProductGroup = FindGroupByName(product.groupName);
                if (currentProductGroup != null)
                {
                    RemoveProductFromGroup(currentProductGroup, product);
                }
                product.groupName = productGroup.name;
                productGroup.products.Add(product);
                return true;
            }
            return false;
        }


        public bool RemoveProductFromGroup(ProductGroup productGroup, Product product)
        {
            if (productGroup.products.Contains(product))
            {
                productGroup.products.Remove(product);
                product.groupName = FindGroupByName(defaultGroup).name;
                return true;
            }
            return false;
        }

        public ProductGroup FindGroupByName(string name)
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

        public Product FindProductByName(string name)
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

                using (StreamReader streamReader = new StreamReader(groupPath))
                {
                    using (CsvReader csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        _productGroups = csvReader.GetRecords<ProductGroup>().ToList();
                    }
                }

                using (StreamReader streamReader = new StreamReader(productsFilePath))
                {
                    using (CsvReader csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        _products = csvReader.GetRecords<Product>().ToList();
                    }
                }
                for (int i = 0; i < _productGroups.Count; i++)
                {
                    _productGroups[i].products = new List<Product>();
                    Console.WriteLine(_productGroups[i]);
                }
                for (int i = 0; i < _products.Count; i++)
                {
                    Console.WriteLine(_products[i].groupName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine(_products.Count);
            return true;
        }
    }
}
