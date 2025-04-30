using System;

namespace ProductStorage
{
    public class Product
    {
        public string name { set; get; }
        public uint price { set; get; }
        public uint id { set; get; }
        public int barcode { set; get; }
        public string groupName { set; get; }
        public string description { set; get; }
        public string unit { set; get; }
        public string country { set; get; }
        public string manufacturer { set; get; }
    }
}

