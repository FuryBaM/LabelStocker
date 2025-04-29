using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using CsvHelper.Configuration;
using System.Globalization;

namespace GoodStorage
{
    public class GoodDatabase
    {
        private List<GoodGroup> _goodGroups = new List<GoodGroup>();
        private List<Good> _goods = new List<Good>();
        public List<Good> Goods { get { return _goods; } private set { } }
        public delegate void GoodCreation(Good good);
        public event GoodCreation OnGoodCreate;
        public event GoodCreation OnGoodRemove;

        public static string defaultGroup = "Прочее";

        public GoodDatabase()
        {
            Load();
            if (_goodGroups.Where(x=>x.name == defaultGroup).Count() == 0)
            {
                AddGroup(new GoodGroup { name = defaultGroup, goods = new List<Good>(), id = 0 });
            }
        }

        public List<GoodGroup> GetGroups() => _goodGroups;

        public bool AddGroup(GoodGroup goodGroup)
        {
            int maxId = 0;
            if (_goodGroups.Count > 0)
                maxId = _goodGroups.Max(x => x.id);
            if (_goodGroups.Where(x => x.name.ToUpper() == goodGroup.name.ToUpper()).Count() != 0)
            {
                MessageBox.Show("Такая группа уже есть в списке.");
                return false;
            }
            else
            {
                _goodGroups.Add(goodGroup);
                goodGroup.id = maxId + 1;
                return true;
            }
        }

        public bool RemoveGroup(GoodGroup group)
        {
            if (group.name == defaultGroup)
            {
                MessageBox.Show("Нельзя удалять данную группу.");
                return false;
            }
            if (_goodGroups.Contains(group))
            {
                _goodGroups.Remove(group);
                return true;
            }
            return false;
        }

        public void AddGood(Good good)
        {
            int maxId = 0;
            if (_goods.Count > 0)
                maxId = Convert.ToInt32(_goods.Max(x => x.id));
            if (_goods.Select(x=>x.id == good.id).Count() != 0)
            {
                good.id = Convert.ToUInt32(maxId) + 1;
            }
            good.groupName = GetGroupWithName(defaultGroup).name;
            _goods.Add(good);
            OnGoodCreate?.Invoke(good);
        }

        public bool RemoveGood(Good good)
        {
            if (_goods.Contains(good))
            {
                _goods.Remove(good);
                OnGoodRemove?.Invoke(good);
                return true;
            }
            return false;
        }

        public bool AddToGroup(GoodGroup goodGroup, Good good)
        {
            if (!goodGroup.goods.Contains(good))
            {
                var currentGoodGroup = GetGroupWithName(good.groupName);
                if (currentGoodGroup != null)
                {
                    RemoveFromGroup(currentGoodGroup, good);
                }
                good.groupName = goodGroup.name;
                goodGroup.goods.Add(good);
                return true;
            }
            return false;
        }


        public bool RemoveFromGroup(GoodGroup goodGroup, Good good)
        {
            if (goodGroup.goods.Contains(good))
            {
                goodGroup.goods.Remove(good);
                good.groupName = GetGroupWithName(defaultGroup).name;
                return true;
            }
            return false;
        }

        public GoodGroup GetGroupWithName(string name)
        {
            try
            {
                return _goodGroups.Single(x => x.name == name);
            }
            catch
            {
                return null;
            }
        }

        public Good GetGoodWithName(string name)
        {
            try
            {
                return _goods.Single(x => x.name == name);
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
                    csvWriter.WriteRecords(_goodGroups);
                }
            }
            string goodPath = AppDomain.CurrentDomain.BaseDirectory + "goods.csv";
            using (StreamWriter streamWriter = new StreamWriter(goodPath))
            {
                using (CsvWriter csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    csvWriter.WriteRecords(_goods);
                }
            }
            Console.WriteLine("Data saved");
        }

        public bool Load()
        {
            try
            {
                string groupPath = AppDomain.CurrentDomain.BaseDirectory + "groups.csv";
                string goodPath = AppDomain.CurrentDomain.BaseDirectory + "goods.csv";

                if (!File.Exists(groupPath) || !File.Exists(goodPath)) return false;

                List<GoodGroup> groups;
                using (StreamReader streamReader = new StreamReader(groupPath))
                {
                    using (CsvReader csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        groups = csvReader.GetRecords<GoodGroup>().ToList();
                    }
                }

                List<Good> goods;
                using (StreamReader streamReader = new StreamReader(goodPath))
                {
                    using (CsvReader csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        goods = csvReader.GetRecords<Good>().ToList();
                    }
                }
                for (int i = 0; i < groups.Count; i++)
                {
                    groups[i].goods = new List<Good>();
                    Console.WriteLine(groups[i]);
                }
                _goodGroups = groups;
                for (int i = 0; i < goods.Count; i++)
                {
                    Console.WriteLine(goods[i].groupName);
                    //AddToGroup(GetGroupWithName(goods[i].group.name), goods[i]);
                }
                _goods = goods;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return true;
        }
    }
}
