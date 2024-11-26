using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TodoManager.src
{
    internal class RootManager
    {
        private List<Board> boards = new List<Board>();

        public RootManager() 
        {
            loadBoards();
            if (boards.Count == 0) //Add Root Board if no boards are loaded
            {
                boards.Add(new Board("Root", true));
            }
        }

        public List<Board> getBoards()
        {
            return boards; 
        }

        public void addBoard(string name, bool active)
        {
            boards.Add(new Board(name, active));
            saveBoards();
        }
        private void saveBoards()
        {
            string fileName = "Boards.csv"; // Specify the file name
            string path = @"D:\" + fileName; // Specify the output path (change as needed)

            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (Board board in boards)
                {
                    writer.WriteLine(board.Name + "," + board.Active);
                }
            }
        }

        private void loadBoards()
        {

            string fileName = "Root.csv"; // Specify the file name
            string path = @"D:\" + fileName; // Specify the output path (change as needed)

            if (!File.Exists(path))
            {
                return;
            }

            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(",");
                    if (parts.Length == 2)
                    {
                        bool activeFormatted = bool.Parse(parts[1]);
                        addBoard(parts[0], activeFormatted);
                    }
                }
            }

        }
    }
}
