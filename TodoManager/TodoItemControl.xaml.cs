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
            if (Parent == TodoContainer)
            {
                MoveLeftButton.IsEnabled = false; // Cannot move left
                MoveRightButton.IsEnabled = true;
            }
            else if (Parent == DoneContainer)
            {
                MoveLeftButton.IsEnabled = true;
                MoveRightButton.IsEnabled = false; // Cannot move right
            }
            else
            {
                MoveLeftButton.IsEnabled = true;
                MoveRightButton.IsEnabled = true;
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
    }
}
