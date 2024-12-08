﻿using System.Diagnostics;
using System.IO;

namespace TodoManager.src
{
    internal class RootManager
    {
        private Dictionary<string, bool> boards = new Dictionary<string, bool>();
        private string activeBoard = string.Empty;

        public RootManager() 
        {
            loadBoards();
            if (boards.Count == 0) //Add Root Board if no boards are loaded
            {
                addBoard("Root", true);
                saveBoardList();
                activeBoard = "Root";
            }
        }

        public string ActiveBoard
        { 
            get { return activeBoard; }
            set { activeBoard = value; }
        }

        public Dictionary<string, bool> getBoards()
        {
            return boards; 
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
            string fileName = "Boards.csv"; // Specify the file name
            string path = @"D:\" + fileName; // Specify the output path (change as needed)

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

        private void loadBoards()
        {

            string fileName = "Boards.csv"; // Specify the file name
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

        public void markActiveBoard(string newActiveBoard, bool deletion)
        {
            boards[activeBoard] = false; // Reset old active boards active value
            boards[newActiveBoard] = true; // Set new active boards active value
            activeBoard = newActiveBoard;
            
            if (!deletion) //This fixes a bug where deleted items are added back into the Boards.csv but introduces a bug where root is marked as active but this is not saved
            {
                saveBoardList(); // Save boards list
            }
        }
    }
}
