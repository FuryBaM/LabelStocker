using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductStorage
{
    public class ProductGroup
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<Product> products { get; set; }
    }
}
