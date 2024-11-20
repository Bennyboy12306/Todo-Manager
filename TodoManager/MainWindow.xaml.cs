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
        public MainWindow()
        {
            InitializeComponent();
            src.Board board = manager.getBoards()[0];
        }

        private void btnAddTodo_Click(object sender, RoutedEventArgs e)
        {
            TodoItemControl newTodoItem = new TodoItemControl
            {
                Title = "New Todo Item",
                Description = "Description of the item",
                TodoContainer = TodoContainer,
                InProgressContainer = InProgressContainer,
                DoneContainer = DoneContainer
            };

            // Add the new control to the Todo container
            TodoContainer.Children.Add(newTodoItem);

            // Initialize button states
            newTodoItem.UpdateButtonStates();
        }
    }
}