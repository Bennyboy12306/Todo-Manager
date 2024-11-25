using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoManager.src
{
    internal class RootManager
    {
        private List<Board> boards = new List<Board>();

        public RootManager() 
        {
            //Debug
            boards.Add(new Board(0, "Root", "Root"));
        }

        public List<Board> getBoards()
        {
            return boards; 
        }
    }
}
