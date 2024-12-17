using System.Diagnostics;
using System.IO;

namespace TodoManager.src
{
    internal class BoardManager
    {
        private string saveDirectory;

        private Dictionary<string, bool> boards = new Dictionary<string, bool>();
        private string activeBoard = string.Empty;

        /// <summary>
        /// This method gets and sets the save directory
        /// </summary>
        public string SaveDirectory
        {
            get { return saveDirectory; }
            set { saveDirectory = value; }
        }

        /// <summary>
        /// This method gets and sets the active board
        /// </summary>
        public string ActiveBoard
        { 
            get { return activeBoard; }
            set { activeBoard = value; }
        }

        /// <summary>
        /// This method gets a dictionary of board names and their active states
        /// </summary>
        /// <returns>A dictionary of board names and their active states</returns>
        public Dictionary<string, bool> getBoards()
        {
            return boards; 
        }

        /// <summary>
        /// This method handles loading board or adding a root board and setting it to active if no boards exist when the program starts up
        /// </summary>
        public void initialLoad()
        {
            loadBoards();
            if (boards.Count == 0) //Add Root Board if no boards are loaded
            {
                addBoard("Root", true);
                saveBoardList();
                activeBoard = "Root";
            }
        }

        /// <summary>
        /// This method handles adding a new board to the list of boards
        /// </summary>
        /// <param name="name">The name of the new board</param>
        /// <param name="active">If this board is active or not</param>
        public void addBoard(string name, bool active)
        {
            boards[name] = active;
        }

        /// <summary>
        /// This method handles removing a board from the list of active boards
        /// </summary>
        /// <param name="name">The name of the board to remove</param>
        public void deleteBoard(string name)
        {
            boards.Remove(name);
        }

        /// <summary>
        /// This method handles saving the list of boards and their active states
        /// </summary>
        public void saveBoardList()
        {
            string fileName = "Boards.csv"; // Specify the file name
            string path = saveDirectory + fileName; // Specify the output path (change as needed)

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

        /// <summary>
        /// This method handles loading the list of boards from the Boards.csv file and marking the correct board as active
        /// </summary>
        private void loadBoards()
        {

            string fileName = "Boards.csv"; // Specify the file name
            string path = saveDirectory + fileName; // Specify the output path (change as needed)

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
            saveBoardList();

        }

        /// <summary>
        /// This method handles marking a board as active
        /// </summary>
        /// <param name="newActiveBoard">The name of the new active board</param>
        public void markActiveBoard(string newActiveBoard)
        {

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

            // Save the updated board list
            saveBoardList();
        }
    }
}
