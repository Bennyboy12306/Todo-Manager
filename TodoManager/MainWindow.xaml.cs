using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace TodoManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string saveDirectory;

        private src.BoardManager manager = new src.BoardManager();
        private string displayedBoard;

        private int items = 0;

        private bool autoSettingBoardSelector = true;

        private bool linkSet = false;
        public MainWindow()
        {
            InitializeComponent();

            LoadConfig();

            manager.loadBoards();

            displayedBoard = manager.ActiveBoard;
            loadAllItems();
            updateBoardSelector();
            autoSettingBoardSelector = false;
        }

        private void LoadConfig()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "TodoManager");
            string configFile = Path.Combine(appFolder, "config.txt");

            try
            {
                if (File.Exists(configFile))
                {
                    // Read the file and return its contents as the file path
                    string directory = File.ReadAllText(configFile).Trim(); // Trim to remove extra whitespace or newlines
                    txtFileLocation.Text = directory;
                    manager.SaveDirectory = directory;
                    saveDirectory = directory;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading configuration: " + ex.Message);
            }
        }

        private void SaveConfig(string content)
        {

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "TodoManager");
            string configFile = Path.Combine(appFolder, "config.txt");

            try
            {
                string directory = Path.GetDirectoryName(configFile);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory); // Ensure the directory exists
                }

                // If it does not end in a backslash add one
                if (!content.EndsWith("\\"))
                {
                    content += "\\";
                }

                // Write the file path to the configuration file
                txtFileLocation.Text = content;
                saveDirectory = content;

                TodoContainer.Children.Clear();
                InProgressContainer.Children.Clear();
                DoneContainer.Children.Clear();

                manager.reset(content);

                loadAllItems();

                //TODO Fix sometimes data gets pulled from one root to another when switching directory

                updateBoardSelector();
                File.WriteAllText(configFile, content);
                Debug.WriteLine("Configuration saved successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving configuration: " + ex.Message);
            }
        }

        private void updateBoardSelector()
        {
            cbbBoardSelect.Items.Clear();
            cbbLinkBoardSelect.Items.Clear();

            Dictionary<string, bool> boards = manager.getBoards();

            foreach (var board in boards)
            {
                string boardName = board.Key;
                bool active = board.Value;

                cbbBoardSelect.Items.Add(boardName);
                cbbLinkBoardSelect.Items.Add(boardName);

                if (active)
                {
                    cbbBoardSelect.SelectedItem = boardName;
                }
            }
            
        }

        private void btnAddTodo_Click(object sender, RoutedEventArgs e)
        {
            // Create a new TodoItemControl instance
            TodoItemControl newTodoItem = new TodoItemControl
            {
                Title = "New Todo Item " + items,
                Description = "Description of the item",
                TodoContainer = TodoContainer,
                InProgressContainer = InProgressContainer,
                DoneContainer = DoneContainer,
                StartDate = DateTime.Now,
                EndDate = null
            };

            // Subscribe to the ItemMoved event
            newTodoItem.ItemMoved += saveAllItems;

            // Subscribe to the LinkButtonClicked event
            newTodoItem.LinkButtonClicked += linkButton;

            // Add the new control to the Todo container
            TodoContainer.Children.Add(newTodoItem);

            saveAllItems();

            items++;
        }

        private void saveAllItems()
        {
            string fileName = displayedBoard + ".csv"; // Specify the file name
            string path = saveDirectory + fileName; // Specify the output path (change as needed)

            using (StreamWriter writer = new StreamWriter(path))
            {
                saveContainerContents(TodoContainer, "Todo", writer);
                saveContainerContents(InProgressContainer, "InProgress", writer);
                saveContainerContents(DoneContainer, "Done", writer);
            }
        }

        private void saveContainerContents(Panel container, string containerName, StreamWriter writer)
        {
            foreach (var child in container.Children)
            {
                if (child is TodoItemControl todoItem)
                {
                    string endDateFormatted = todoItem.EndDate?.ToString() ?? "Ongoing";
                    writer.WriteLine(containerName + "," + todoItem.Title + "," + todoItem.Description + "," + todoItem.StartDate + "," + endDateFormatted + "," + todoItem.LinkedBoard);
                }
            }
        }

        private void loadAllItems()
        {

            string fileName = displayedBoard + ".csv"; // Specify the file name
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
                    if (parts.Length == 6)
                    {
                        string containerName = parts[0];
                        string title = parts[1];
                        string description = parts[2];
                        DateTime startDate = DateTime.Parse(parts[3]);
                        DateTime? endDate = parts[4] == "Ongoing" ? (DateTime?)null : DateTime.Parse(parts[4]);
                        string linkedBoard = parts[5];

                        loadContainerContents(containerName, title, description, startDate, endDate, linkedBoard);
                    }
                }
            }
        }

        private void loadContainerContents(string ContainerName, string title, string description, DateTime startDate, DateTime? endDate, string linkedBoard)
        {

            TodoItemControl newTodoItem = new TodoItemControl
            {
                Title = title,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                LinkedBoard = linkedBoard,
                TodoContainer = TodoContainer,
                InProgressContainer = InProgressContainer,
                DoneContainer = DoneContainer
            };

            // Subscribe to the ItemMoved event
            newTodoItem.ItemMoved += saveAllItems;

            // Subscribe to the LinkButtonClicked event
            newTodoItem.LinkButtonClicked += linkButton;

            Panel targetContainer = ContainerName switch
            {
                "Todo" => TodoContainer,
                "InProgress" => InProgressContainer,
                "Done" => DoneContainer,
                _ => null
            };

            if (targetContainer != null)
            {
                targetContainer.Children.Add(newTodoItem);
            }

        }

        private void Column_DragEnter(object sender, DragEventArgs e)
        {
            if (sender is ScrollViewer column)
            {
                column.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF610000"));
            }
        }

        private void Column_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is ScrollViewer column)
            {
                column.Background = Brushes.Transparent;
            }
        }


        private void Column_DragOver(object sender, DragEventArgs e)
        {
            // Allow moving the item if it is valid
            if (e.Data.GetDataPresent(typeof(TodoItemControl)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void Column_Drop(object sender, DragEventArgs e)
        {
            if (sender is ScrollViewer column)
            {
                column.Background = Brushes.Transparent;
            }

            if (e.Data.GetDataPresent(typeof(TodoItemControl)))
            {
                TodoItemControl draggedItem = (TodoItemControl)e.Data.GetData(typeof(TodoItemControl));

                // Check if draggedItem's Parent is a StackPanel
                if (draggedItem.Parent is StackPanel currentContainer)
                {
                    // Check if the sender's Content is a StackPanel
                    if (((ScrollViewer)sender).Content is StackPanel targetContainer)
                    {
                        // Abort if the current and target containers are the same
                        if (currentContainer == targetContainer)
                        {
                            return;
                        }

                        // Remove the dragged item from the current container
                        currentContainer.Children.Remove(draggedItem);

                        Point dropPosition = e.GetPosition(targetContainer);
                        int insertIndex = targetContainer.Children.Count;

                        // Insert dragged item into the target container
                        targetContainer.Children.Insert(insertIndex, draggedItem);

                        saveAllItems();
                    }
                }
            }
        }

        private void btnDeleteBoard_Click(object sender, RoutedEventArgs e)
        {
            string fileName = displayedBoard + ".csv"; // Specify the file name
            string path = saveDirectory + fileName; // Specify the output path (change as needed)

            if (displayedBoard.Equals("Root"))
            {
                TodoContainer.Children.Clear();
                InProgressContainer.Children.Clear();
                DoneContainer.Children.Clear();
                saveAllItems();
                sendLog("Cannot delete root board, Clearing items instead", false);
                return;
            }

            File.Delete(path);
            manager.deleteBoard(displayedBoard);

            autoSettingBoardSelector = true;
            updateBoardSelector();
            autoSettingBoardSelector = false;

            manager.saveBoardList();

            switchActiveBoard("Root", true);

            sendLog("Deleted: " + displayedBoard, false);
        }

        private void btnAddBoard_Click(object sender, RoutedEventArgs e)
        {
            string newBoardName = NewBoardNameText.Text;

            if (manager.getBoards().ContainsKey(newBoardName))
            {
                sendLog("Error: A board with this name already exists", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(newBoardName) || newBoardName.Contains(" "))
            {
                sendLog("Error: Board names cannot be empty or contain a space", true);
                return;
            }

            if (newBoardName.Equals("Boards"))
            {
                sendLog("Error: This name is not allowed", true);
                return;
            }

            manager.addBoard(newBoardName, false);
            manager.saveBoardList();
            autoSettingBoardSelector = true;
            updateBoardSelector();
            autoSettingBoardSelector = false;

            sendLog("Added new board: " + newBoardName, false);
        }

        private void cbbBoardSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!autoSettingBoardSelector)
            {
                switchActiveBoard(null, false);
            }
        }

        private void switchActiveBoard(string? linkedBoardName, bool deletion)
        {
            if (!deletion)
            {
                saveAllItems();
            }
            TodoContainer.Children.Clear();
            InProgressContainer.Children.Clear();
            DoneContainer.Children.Clear();

            // Update the active board
            if (linkedBoardName != null)
            {
                displayedBoard = linkedBoardName;
                autoSettingBoardSelector = true;
                cbbBoardSelect.SelectedItem = displayedBoard;
                autoSettingBoardSelector = false;
            }
            else
            {
                displayedBoard = cbbBoardSelect.SelectedItem != null ? cbbBoardSelect.SelectedItem.ToString() : "Root";
            }
            manager.markActiveBoard(displayedBoard, deletion);

            sendLog("Switched active board to: " + displayedBoard, false);

            // Load the items for the newly selected board
            loadAllItems();
        }

        private void cbbLinkModeSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!autoSettingBoardSelector)
            {
                linkSet = !linkSet;
            }
        }

        private void linkButton(string linkedBoardName, TodoItemControl todoItem)
        {
            if (!linkSet)
            {
                if (string.IsNullOrWhiteSpace(linkedBoardName))
                {
                    sendLog("Error: No linked board set", true);
                    return;
                }
                if (manager.getBoards().Keys.Contains(linkedBoardName))
                {
                    switchActiveBoard(linkedBoardName, false);
                    sendLog("Opened: " + linkedBoardName + " from link", false);
                    return;
                }
                sendLog("Error: Could not find board to open from link", true);
            }
            else
            {
                todoItem.LinkedBoard = cbbLinkBoardSelect.Text;
                sendLog("Linked item to: " + cbbLinkBoardSelect.Text + " board", false);
            }
        }

        private void sendLog(string message, bool error)
        {
            if (error)
            {
                txtInfo.Foreground = Brushes.Red;
            }
            else
            {
                txtInfo.Foreground = Brushes.White;
            }
            txtInfo.Content = message;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Create an OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set to validate paths and not require a file selection
            openFileDialog.ValidateNames = false; // Allow selecting folders
            openFileDialog.CheckFileExists = false; // Do not check if file exists
            openFileDialog.CheckPathExists = true; // Ensure path exists
            openFileDialog.FileName = "Select Folder"; // Set default file name as a placeholder

            // Show the dialog and get result
            if (openFileDialog.ShowDialog() == true) // Returns true if the user selects a location
            {
                // Extract the folder path
                string selectedFolderPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                MessageBox.Show($"You selected: {selectedFolderPath}");
                SaveConfig(selectedFolderPath);
            }
        }
    }
}