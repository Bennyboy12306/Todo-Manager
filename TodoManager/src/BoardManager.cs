using System.Diagnostics;
using System.IO;

namespace TodoManager.src
{
    internal class BoardManager
    {
        private string saveDirectory;

        private Dictionary<string, bool> boards = new Dictionary<string, bool>();
        private string activeBoard = string.Empty;

        public string SaveDirectory
        {
            set { saveDirectory = value; }
        }

        public Dictionary<string, bool> getBoards()
        {
            return boards; 
        }

        public string ActiveBoard
        {
            get { return activeBoard; }
            set { activeBoard = value; }
        }

        public void reset(string newSaveDirectory)
        {
            saveDirectory = newSaveDirectory;
            boards.Clear();
            activeBoard = string.Empty;
            loadBoards();
        }

        public void addBoard(string name, bool active)
        {
            boards[name] = active;
        }

        public void deleteBoard(string name)
        {
            boards.Remove(name);
        }

        public void saveBoardList()
        {
            string path = saveDirectory + "Boards.csv";

            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (var board in boards)
                {
                    string name = board.Key;
                    bool active = board.Value;
                    writer.WriteLine(name + "," + active);
                }
            }
        }

        public void loadBoards()
        {
            string path = saveDirectory + "Boards.csv";

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
                        string boardName = parts[0];
                        bool activeFormatted = bool.Parse(parts[1]);
                        if (activeFormatted)
                        {
                            activeBoard = boardName;
                        }
                        addBoard(boardName, activeFormatted);
                    }
                }
            }

            if (boards.Count == 0) //Add Root Board if no boards are loaded
            {
                addBoard("Root", true);
                activeBoard = "Root";
                saveBoardList();
            }
        }

        public void markActiveBoard(string newActiveBoard, bool deletion)
        {
            Console.WriteLine($"Marking '{newActiveBoard}' as active. Deletion: {deletion}");

            // Update the active status in memory
            if (boards.ContainsKey(activeBoard))
            {
                boards[activeBoard] = false; // Reset the old active board
            }

            if (boards.ContainsKey(newActiveBoard))
            {
                boards[newActiveBoard] = true; // Set the new active board
            }

            activeBoard = newActiveBoard;

            // Save the updated board list, regardless of deletion
            saveBoardList();
        }
    }
}
