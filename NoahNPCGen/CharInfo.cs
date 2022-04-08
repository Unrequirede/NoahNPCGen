using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoahNPCGen
{
    public class CharInfo
    {
        public string Name { get; set; }
        public string Race { get; set; }
        public string Class { get; set; }
        public int Level { get; set; }
        public void OnGetSingleOrder(int orderId)
        {
            Level = orderId;
        }
    }
}
