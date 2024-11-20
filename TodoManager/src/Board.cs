using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoManager.src
{
    class Board : Item
    {
        public Board(int id) : base(id)
        {
        }

        public Board(int id, string name, string description) : base(id, name, description)
        {
        }
    }
}
