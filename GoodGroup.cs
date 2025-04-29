using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodStorage
{
    public class GoodGroup
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<Good> goods { get; set; }
    }
}
