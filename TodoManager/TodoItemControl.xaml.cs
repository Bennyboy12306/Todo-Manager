using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TodoManager
{
    public partial class TodoItemControl : UserControl
    {
        private Point dragStartPoint;

        // Public properties for Title and Description
        public string Title
        {
            get => TitleText.Text;
            set => TitleText.Text = value;
        }

        public string Description
        {
            get => DescriptionText.Text;
            set => DescriptionText.Text = value;
        }

        // References to the parent containers
        public StackPanel TodoContainer { get; set; }
        public StackPanel InProgressContainer { get; set; }
        public StackPanel DoneContainer { get; set; }

        public TodoItemControl()
        {
            InitializeComponent();
        }

        // Update button states (enable/disable based on current container)
        public void UpdateButtonStates()
        {
            if (Parent is StackPanel parentPanel)
            {
                // Get all the items in the container (excluding headers)
                var siblings = parentPanel.Children.OfType<TodoItemControl>().ToList();

                // Find the current position of this item within its container
                int currentIndex = siblings.IndexOf(this);

                // Enable or disable the MoveLeftButton and MoveRightButton based on the container
                if (parentPanel.Name == "TodoContainer")
                {
                    MoveLeftButton.IsEnabled = false; // Cannot move left
                    MoveRightButton.IsEnabled = true;
                }
                else if (parentPanel.Name == "DoneContainer")
                {
                    MoveLeftButton.IsEnabled = true;
                    MoveRightButton.IsEnabled = false; // Cannot move right
                }
                else
                {
                    MoveLeftButton.IsEnabled = true;
                    MoveRightButton.IsEnabled = true;
                }

                // Enable or disable the MoveUpButton and MoveDownButton based on position
                MoveUpButton.IsEnabled = currentIndex > 0; // Disable if it's the first item
                MoveDownButton.IsEnabled = currentIndex < siblings.Count - 1; // Disable if it's the last item
            }
        }

        private void Column_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TodoItemControl)))
            {
                TodoItemControl draggedItem = (TodoItemControl)e.Data.GetData(typeof(TodoItemControl));

                // Remove from the current container
                if (draggedItem.Parent is StackPanel currentContainer)
                {
                    currentContainer.Children.Remove(draggedItem);
                }

                // Add to the new container
                if (((ScrollViewer)sender).Content is StackPanel targetContainer)
                {
                    targetContainer.Children.Add(draggedItem);
                }

                // Update button states
                draggedItem.UpdateButtonStates();
            }
        }

        // Record the start point on mouse down
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                dragStartPoint = e.GetPosition(this);
            }
        }

        // Start the drag-and-drop operation on mouse move
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this);

                // Check if the movement is sufficient to start a drag
                if (Math.Abs(currentPoint.X - dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPoint.Y - dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // Begin drag operation
                    DragDrop.DoDragDrop(this, this, DragDropEffects.Move);
                }
            }
        }


        private void MoveLeft_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent == InProgressContainer)
            {
                // Move to Todo
                InProgressContainer.Children.Remove(this);
                TodoContainer.Children.Add(this);
            }
            else if (this.Parent == DoneContainer)
            {
                // Move to In Progress
                DoneContainer.Children.Remove(this);
                InProgressContainer.Children.Add(this);
            }

            UpdateButtonStates();
        }

        private void MoveRight_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent == TodoContainer)
            {
                // Move to In Progress
                TodoContainer.Children.Remove(this);
                InProgressContainer.Children.Add(this);
            }
            else if (this.Parent == InProgressContainer)
            {
                // Move to Done
                InProgressContainer.Children.Remove(this);
                DoneContainer.Children.Add(this);
            }

            UpdateButtonStates();
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (Parent is StackPanel container)
            {
                int currentIndex = container.Children.IndexOf(this);
                if (currentIndex > 1) // Prevent moving above the header
                {
                    container.Children.Remove(this);
                    container.Children.Insert(currentIndex - 1, this);
                }
                UpdateButtonStates();
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (Parent is StackPanel container)
            {
                int currentIndex = container.Children.IndexOf(this);
                if (currentIndex < container.Children.Count - 1)
                {
                    container.Children.Remove(this);
                    container.Children.Insert(currentIndex + 1, this);
                }
                UpdateButtonStates();
            }
        }

    }
}
