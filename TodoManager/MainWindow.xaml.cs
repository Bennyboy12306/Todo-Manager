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

namespace TodoManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private src.RootManager manager = new src.RootManager();

        private int items = 0;
        public MainWindow()
        {
            InitializeComponent();
            src.Board board = manager.getBoards()[0];
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
                DoneContainer = DoneContainer
            };

            // Add the new control to the Todo container
            TodoContainer.Children.Add(newTodoItem);

            // Initialize button states
            newTodoItem.UpdateButtonStates();

            items++;
        }

        private void Column_DragEnter(object sender, DragEventArgs e)
        {
            // Ensure the dragged item is a TodoItemControl
            if (!e.Data.GetDataPresent(typeof(TodoItemControl)))
            {
                e.Effects = DragDropEffects.None;
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
            if (e.Data.GetDataPresent(typeof(TodoItemControl)))
            {
                // Get the dragged item
                TodoItemControl draggedItem = (TodoItemControl)e.Data.GetData(typeof(TodoItemControl));

                // Remove the item from its current parent
                StackPanel currentParent = draggedItem.Parent as StackPanel;
                if (currentParent != null)
                {
                    currentParent.Children.Remove(draggedItem);
                }

                // Add the item to the target container
                StackPanel targetContainer = ((ScrollViewer)sender).Content as StackPanel;
                if (targetContainer != null)
                {
                    targetContainer.Children.Add(draggedItem);
                }
            }
        }

    }
}