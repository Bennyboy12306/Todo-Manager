using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace TodoManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private src.BoardManager manager = new src.BoardManager();
        private string displayedBoard;

        private int items = 0;

        private bool autoSettingBoardSelector = true;

        private bool linkSet = false;

        /// <summary>
        /// Constructor, Initializes component loads the config gets the active board and displays it, loading all the items then updates the board selectors
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();

            if (manager.SaveDirectory != null && !manager.SaveDirectory.Equals("None\\"))
            {
                manager.initialLoad();
            }

            displayedBoard = manager.ActiveBoard;

            loadAllItems();
            updateBoardSelector();
            autoSettingBoardSelector = false;
        }

        /// <summary>
        /// This method updates the board and link board selector combobox with the available boards
        /// </summary>
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

        /// <summary>
        /// This method creates a new todo item, adds it into the todo column and then saves when the button is clicked
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void btnAddTodo_Click(object sender, RoutedEventArgs e)
        {
            if (manager.SaveDirectory != null && !manager.SaveDirectory.Equals("None\\"))
            {
                // Create a new TodoItemControl instance
                TodoItemControl newTodoItem = new TodoItemControl(TodoContainer, InProgressContainer, DoneContainer)
                {
                    Title = "New Todo Item " + items,
                    Description = "Description of the item",
                    StartDate = null,
                    EndDate = null
                };

                // Subscribe to the ItemChanged event (Will cause items to be saved any time one is updated)
                newTodoItem.ItemChanged += saveAllItems;

                // Subscribe to the LinkButtonClicked event
                newTodoItem.LinkButtonClicked += linkButton;

                // Add the new control to the Todo container
                TodoContainer.Children.Add(newTodoItem);

                saveAllItems();

                items++;
            }
            else
            {
                sendLog("Error: Please set a file location first.", true);
            }
        }

        /// <summary>
        /// This method saves all of the items from each container
        /// </summary>
        private void saveAllItems()
        {
            string fileName = displayedBoard + ".csv"; // Specify the file name
            string path = manager.SaveDirectory + fileName; // Specify the output path (change as needed)

            using (StreamWriter writer = new StreamWriter(path))
            {
                saveContainerContents(TodoContainer, "Todo", writer);
                saveContainerContents(InProgressContainer, "InProgress", writer);
                saveContainerContents(DoneContainer, "Done", writer);
            }
        }

        /// <summary>
        /// This method handles saving items in a specific container
        /// </summary>
        /// <param name="container">The container to save items from</param>
        /// <param name="containerName">The name of the container to save items from</param>
        /// <param name="writer">The stream writer object</param>
        private void saveContainerContents(Panel container, string containerName, StreamWriter writer)
        {
            foreach (var child in container.Children)
            {
                if (child is TodoItemControl todoItem)
                {
                    string startDateFormatted = todoItem.StartDate?.ToString() ?? "NotStarted";
                    string endDateFormatted = todoItem.EndDate?.ToString() ?? "Ongoing";
                    writer.WriteLine(containerName + "," + todoItem.Title + "," + todoItem.Description + "," + startDateFormatted + "," + endDateFormatted + "," + todoItem.LinkedBoard);
                }
            }
        }

        /// <summary>
        /// This method loads all of the items from each container
        /// </summary>
        private void loadAllItems()
        {

            string fileName = displayedBoard + ".csv"; // Specify the file name
            string path = manager.SaveDirectory + fileName; // Specify the output path (change as needed)

            if (!File.Exists(path))
            {
                return;
            }

            using (StreamReader reader = new StreamReader(path))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(",");
                    if (parts.Length == 6)
                    {
                        string containerName = parts[0];
                        string title = parts[1];
                        string description = parts[2];
                        DateTime? startDate = parts[3] == "NotStarted" ? (DateTime?)null : DateTime.Parse(parts[3]);
                        DateTime? endDate = parts[4] == "Ongoing" ? (DateTime?)null : DateTime.Parse(parts[4]);
                        string linkedBoard = parts[5];

                        loadContainerContents(containerName, title, description, startDate, endDate, linkedBoard);
                    }
                }
            }
        }

        /// <summary>
        /// This method handles loading items from a specific container
        /// </summary>
        /// <param name="ContainerName">The name of the container to load</param>
        /// <param name="title">The items title</param>
        /// <param name="description">The items description</param>
        /// <param name="startDate">The items start date</param>
        /// <param name="endDate">The items end date</param>
        /// <param name="linkedBoard">The items linked board</param>
        private void loadContainerContents(string ContainerName, string title, string description, DateTime? startDate, DateTime? endDate, string linkedBoard)
        {

            TodoItemControl newTodoItem = new TodoItemControl(TodoContainer, InProgressContainer, DoneContainer)
            {
                Title = title,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                LinkedBoard = linkedBoard,
            };

            // Subscribe to the ItemChanged event (Will cause items to be saved any time one is updated)
            newTodoItem.ItemChanged += saveAllItems;

            // Subscribe to the LinkButtonClicked event
            newTodoItem.LinkButtonClicked += linkButton;

            Panel? targetContainer = ContainerName switch
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

        /// <summary>
        /// This method changes the background color of a container when an item is dragged over it
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void Column_DragEnter(object sender, DragEventArgs e)
        {
            if (sender is ScrollViewer column)
            {
                column.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF610000"));
            }
        }

        /// <summary>
        /// This method resets the background color of a container back after an item is dragged out from this container
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void Column_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is ScrollViewer column)
            {
                column.Background = Brushes.Transparent;
            }
        }

        /// <summary>
        /// This method updates the cursor based on whether or not moving the item is allowed
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
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

        /// <summary>
        /// This method handles dropping an item into a new column
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
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

        /// <summary>
        /// This method handles deleting a board when the button is clicked (Root just gets cleared instead of deleted)
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void btnDeleteBoard_Click(object sender, RoutedEventArgs e)
        {
            if (manager.SaveDirectory != null && !manager.SaveDirectory.Equals("None\\"))
            {
                string fileName = displayedBoard + ".csv"; // Specify the file name
                string path = manager.SaveDirectory + fileName; // Specify the output path (change as needed)

                // If the board is the root board just clear it and save the empty file
                if (displayedBoard.Equals("Root"))
                {
                    TodoContainer.Children.Clear();
                    InProgressContainer.Children.Clear();
                    DoneContainer.Children.Clear();
                    saveAllItems();
                    sendLog("Cannot delete root board, Clearing items instead", false);
                    return;
                }

                // Otherwise delete the file, update the board selectors then switch the active board back to the root board
                File.Delete(path);
                manager.deleteBoard(displayedBoard);

                autoSettingBoardSelector = true;
                updateBoardSelector();
                autoSettingBoardSelector = false;

                manager.saveBoardList();

                switchActiveBoard("Root", true);

                sendLog("Deleted: " + displayedBoard, false);
            }
            else
            {
                sendLog("Error: Please set a file location first.", true);
            }
        }

        /// <summary>
        /// This method handles adding a new board when the button is clicked (Boards must have unique names that are not empty and do not contain a space, The name "boards" is used for storing a list of all the boards so trying to make a board with this name is not allowed)
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void btnAddBoard_Click(object sender, RoutedEventArgs e)
        {
            if (manager.SaveDirectory != null && !manager.SaveDirectory.Equals("None\\"))
            {
                string newBoardName = NewBoardNameText.Text;

                // Ensure board name is unique
                if (manager.getBoards().ContainsKey(newBoardName))
                {
                    sendLog("Error: A board with this name already exists", true);
                    return;
                }

                // Ensure name is not empty and does not contain a space
                if (string.IsNullOrWhiteSpace(newBoardName) || newBoardName.Contains(" "))
                {
                    sendLog("Error: Board names cannot be empty or contain a space", true);
                    return;
                }

                // Ensure board name is not "Boards" This is used by the program for storing a list of all available boards
                if (newBoardName.Equals("Boards"))
                {
                    sendLog("Error: This name is not allowed", true);
                    return;
                }

                // If program reaches this point, all above checks have passed so add the new board
                manager.addBoard(newBoardName, false);
                manager.saveBoardList();
                autoSettingBoardSelector = true;
                updateBoardSelector();
                autoSettingBoardSelector = false;

                sendLog("Added new board: " + newBoardName, false);
            }
            else
            {
                sendLog("Error: Please set a file location first.", true);
            }
        }

        /// <summary>
        /// This method handles switching the active board when the combobox selection is changed
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void cbbBoardSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!autoSettingBoardSelector)
            {
                switchActiveBoard(null, false);
            }
        }

        /// <summary>
        /// This method handles switching the active board
        /// </summary>
        /// <param name="linkedBoardName">The name of the linked board if we are switching to it</param>
        /// <param name="deletion">If we are switching after deleting a board</param>
        private void switchActiveBoard(string? linkedBoardName, bool deletion)
        {
            // As long as we have not just deleted a board save all the items
            if (!deletion)
            {
                saveAllItems();
            }
            // Clear all the containers
            TodoContainer.Children.Clear();
            InProgressContainer.Children.Clear();
            DoneContainer.Children.Clear();

            // Update the active board, If we have a linked board set that to be active, otherwise set the item from the combobox board select to be active
            if (linkedBoardName != null)
            {
                displayedBoard = linkedBoardName;
                autoSettingBoardSelector = true;
                cbbBoardSelect.SelectedItem = displayedBoard;
                autoSettingBoardSelector = false;
            }
            else
            {
                displayedBoard = cbbBoardSelect.SelectedItem.ToString() ?? "";
                if (displayedBoard == "")
                {
                    return;
                }
            }
            // Mark the active board on the manager
            manager.markActiveBoard(displayedBoard);

            sendLog("Switched active board to: " + displayedBoard, false);

            // Load the items for the newly selected board
            loadAllItems();
        }

        /// <summary>
        /// This method switches which link mode we are in when the combobox selection is changed
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void cbbLinkModeSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!autoSettingBoardSelector)
            {
                linkSet = !linkSet;
            }
        }

        /// <summary>
        /// This method handles opening or setting a link on an item, when the button is clicked
        /// </summary>
        /// <param name="linkedBoardName">The name of the linked board</param>
        /// <param name="todoItem">The item object</param>
        private void linkButton(string? linkedBoardName, TodoItemControl todoItem)
        {
            // If linkSet is false we are opening a link
            if (!linkSet)
            {
                // Ensure the linked board name is not null or whitespace
                if (string.IsNullOrWhiteSpace(linkedBoardName))
                {
                    sendLog("Error: No linked board set", true);
                    return;
                }
                // Ensure the board we are trying to link to exists, if so open it
                if (manager.getBoards().Keys.Contains(linkedBoardName))
                {
                    switchActiveBoard(linkedBoardName, false);
                    sendLog("Opened: " + linkedBoardName + " from link", false);
                    return;
                }
                sendLog("Error: Could not find board to open from link", true);
            }
            // If linkSet is true we are linking a board
            else
            {
                // Set the linked board on the item
                todoItem.LinkedBoard = cbbLinkBoardSelect.Text;
                sendLog("Linked item to: " + cbbLinkBoardSelect.Text + " board", false);
            }
        }

        /// <summary>
        /// This method is a simple logger which is used to output messages to the screen
        /// </summary>
        /// <param name="message">The message to output</param>
        /// <param name="error">If this message is an error or not (Determines text color, White/Red)</param>
        private void sendLog(string message, bool error)
        {
            // Set the color based on if the message is an error or not
            if (error)
            {
                txtInfo.Foreground = Brushes.Red;
            }
            else
            {
                txtInfo.Foreground = Brushes.White;
            }
            // Display the message
            txtInfo.Content = message;
        }

        /// <summary>
        /// This method handles browsing files to choose a save directory when the button is clicked
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
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
                string? selectedFolderPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                if (selectedFolderPath != null)
                {
                    MessageBox.Show($"You selected: {selectedFolderPath}");
                    SaveConfig(selectedFolderPath);
                }
            }
        }

        /// <summary>
        /// This method handles loading the save directory from the config in app data (This is how the program remembers where to locate your files after restarting)
        /// </summary>
        private void LoadConfig()
        {
            // Set up the variables to locate the config file
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
                }
                else
                {
                    SaveConfig("None"); //Save a blank config file
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading configuration: " + ex.Message);
            }
        }

        /// <summary>
        /// This method handles saving the save directory to the config in app data
        /// </summary>
        /// <param name="content">The contents (save directory path) to save</param>
        private void SaveConfig(string content)
        {
            // Set up the variables to locate the config file
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "TodoManager");
            string configFile = Path.Combine(appFolder, "config.txt");

            try
            {
                // Ensure the parent directory (appFolder) exists
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                // Add a trailing backslash to the content if not present
                if (!content.EndsWith("\\"))
                {
                    content += "\\";
                }

                // Write the file path to the configuration file
                File.WriteAllText(configFile, content);

                txtFileLocation.Text = content;
                Debug.WriteLine("Configuration saved successfully.");

                // Once we have changed the config and saved it, it is easier to just restart the program rather than try to save and unload all of the current data to load up the data from the new file path
                RestartApplication();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving configuration: " + ex.Message);
            }
        }


        /// <summary>
        /// This method handles restarting the application after the save directory has changed
        /// </summary>
        public static void RestartApplication()
        {
            try
            {
                // Get the full path to the currently running executable
                string? executablePath = Environment.ProcessPath;

                // Start a new instance of the application
                Process.Start(new ProcessStartInfo
                {
                    FileName = executablePath,
                    UseShellExecute = true, // Use shell execution to start the process
                });

                // Exit the current application
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error restarting application: " + ex.Message);
            }
        }
    }
}