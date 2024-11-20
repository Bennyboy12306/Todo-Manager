using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoManager.src
{
    class Item : Details
    {
        public Item(int id) : base(id)
        {
        }

        public Item(int id, string name, string description) : base(id, name, description)
        {
        }
    }
}
