using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoManager.src
{
    class Board
    {
        public string name;
        public bool active;

        public Board(string name, bool active)
        {
            this.name = name;
            this.active = active;
        }

        public string Name { get { return name; } }

        public bool Active { get { return active; } }
    }
}
