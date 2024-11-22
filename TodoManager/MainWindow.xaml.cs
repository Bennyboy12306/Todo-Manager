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
            if (sender is ScrollViewer column)
            {
                column.Background = new SolidColorBrush(Colors.LightBlue);
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

                if (draggedItem.Parent is StackPanel currentContainer)
                {
                    currentContainer.Children.Remove(draggedItem);
                }

                if (((ScrollViewer)sender).Content is StackPanel targetContainer)
                {
                    Point dropPosition = e.GetPosition(targetContainer);
                    int insertIndex = 0;

                    for (int i = 0; i < targetContainer.Children.Count; i++)
                    {
                        FrameworkElement child = targetContainer.Children[i] as FrameworkElement;
                        if (child != null && dropPosition.Y < child.TransformToAncestor(targetContainer).Transform(new Point(0, 0)).Y + child.ActualHeight / 2)
                        {
                            insertIndex = i;
                            break;
                        }
                    }

                    targetContainer.Children.Insert(insertIndex, draggedItem);
                    draggedItem.UpdateButtonStates();
                }
            }
        }


    }
}