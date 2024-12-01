using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TodoManager.src;
using System.IO;

namespace TodoManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private src.RootManager manager = new src.RootManager();
        private string displayedBoard;

        private int items = 0;

        private bool autoSetBoardSelector = false;
        public MainWindow()
        {
            InitializeComponent();
            displayedBoard = manager.ActiveBoard;
            loadAllItems();
            updateBoardSelector();
            autoSetBoardSelector = true;
        }

        private void updateBoardSelector()
        {
            cbbBoardSelect.Items.Clear();

            Dictionary<string, bool> boards = manager.getBoards();

            foreach (var board in boards)
            {
                string boardName = board.Key;
                bool active = board.Value;

                cbbBoardSelect.Items.Add(boardName);

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

            // Add the new control to the Todo container
            TodoContainer.Children.Add(newTodoItem);

            saveAllItems();

            items++;
        }

        private void saveAllItems()
        {
            string fileName = displayedBoard + ".csv"; // Specify the file name
            string path = @"D:\" + fileName; // Specify the output path (change as needed)

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
                    writer.WriteLine(containerName + "," + todoItem.Title + "," + todoItem.Description + "," + todoItem.StartDate + "," + endDateFormatted);
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
                    if (parts.Length == 5)
                    {
                        string containerName = parts[0];
                        string title = parts[1];
                        string description = parts[2];
                        DateTime startDate = DateTime.Parse(parts[3]);
                        DateTime? endDate = parts[4] == "Ongoing" ? (DateTime?)null : DateTime.Parse(parts[4]);

                        loadContainerContents(containerName, title, description, startDate, endDate);
                    }
                }
            }

        }

        private void loadContainerContents(string ContainerName, string title, string description, DateTime startDate, DateTime? endDate)
        {

            TodoItemControl newTodoItem = new TodoItemControl
            {
                Title = title,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                TodoContainer = TodoContainer,
                InProgressContainer = InProgressContainer,
                DoneContainer = DoneContainer
            };

            // Subscribe to the ItemMoved event
            newTodoItem.ItemMoved += saveAllItems;

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

        private void btnAddBoard_Click(object sender, RoutedEventArgs e)
        {
            string newBoardName = NewBoardNameText.Text;

            if (manager.getBoards().ContainsKey(newBoardName))
            {
                txtError.Content = "Error: A board with this name already exists.";
                return;
            }

            if (string.IsNullOrWhiteSpace(newBoardName) || newBoardName.Contains(" "))
            {
                txtError.Content = "Error: Board names cannot be empty or contain a space.";
                return;
            }

            if (newBoardName.Equals("Boards"))
            {
                txtError.Content = "Error: This name is not allowed.";
                return;
            }

            manager.addBoard(newBoardName, false);
            manager.saveBoardList();
            autoSetBoardSelector = false;
            updateBoardSelector();
            autoSetBoardSelector = true;

            txtError.Content = "";
        }

        private void cbbBoardSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (autoSetBoardSelector)
            {
                switchActiveBoard();
            }
        }

        private void switchActiveBoard()
        {
            saveAllItems();
            TodoContainer.Children.Clear();
            InProgressContainer.Children.Clear();
            DoneContainer.Children.Clear();

            // Update the active board
            displayedBoard = cbbBoardSelect.SelectedItem.ToString();
            manager.markActiveBoard(displayedBoard);

            // Load the items for the newly selected board
            loadAllItems();
        }
    }
}