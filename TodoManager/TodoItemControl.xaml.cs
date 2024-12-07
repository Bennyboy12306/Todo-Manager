using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TodoManager
{
    public partial class TodoItemControl : UserControl, INotifyPropertyChanged
    {
        public event Action ItemMoved;

        public event Action<String, TodoItemControl> LinkButtonClicked;

        private Point dragStartPoint;

        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(TodoItemControl), new PropertyMetadata(false));

        public bool IsEditing
        {
            get => (bool)GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }

        private string title;
        private string description;
        private DateTime? startDate;
        private DateTime? endDate;
        private string linkedBoard;

        public event PropertyChangedEventHandler PropertyChanged;

        // Public properties for binding
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public DateTime? StartDate
        {
            get => startDate;
            set
            {
                startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public DateTime? EndDate
        {
            get => endDate;
            set
            {
                endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        public string LinkedBoard
        {
            get => linkedBoard;
            set
            {
                linkedBoard = value;
                OnPropertyChanged(nameof(LinkedBoard));
            }
        }

        // References to the parent containers
        public StackPanel TodoContainer { get; set; }
        public StackPanel InProgressContainer { get; set; }
        public StackPanel DoneContainer { get; set; }

        public TodoItemControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        private void GotFocus(object sender, RoutedEventArgs e)
        {
            IsEditing = true; // User is editing
        }

        private void LostFocus(object sender, RoutedEventArgs e)
        {
            IsEditing = false; // User finished editing
        }

        // Start the drag-and-drop operation on mouse move
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsEditing) return;

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
                // Notify listeners
                ItemMoved?.Invoke();
            }
            else if (this.Parent == DoneContainer)
            {
                // Move to In Progress
                DoneContainer.Children.Remove(this);
                InProgressContainer.Children.Add(this);
                // Notify listeners
                ItemMoved?.Invoke();
            }
        }

        private void MoveRight_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent == TodoContainer)
            {
                // Move to In Progress
                TodoContainer.Children.Remove(this);
                InProgressContainer.Children.Add(this);
                // Notify listeners
                ItemMoved?.Invoke();
            }
            else if (this.Parent == InProgressContainer)
            {
                // Move to Done
                InProgressContainer.Children.Remove(this);
                DoneContainer.Children.Add(this);
                // Notify listeners
                ItemMoved?.Invoke();
            }
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
                    // Notify listeners
                    ItemMoved?.Invoke();
                }
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
                    // Notify listeners
                    ItemMoved?.Invoke();
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (Parent is StackPanel container)
            {
                container.Children.Remove(this);
                // Notify listeners
                ItemMoved?.Invoke();
            }
        }

        private void Link_Click(object sender, RoutedEventArgs e)
        {
            LinkButtonClicked?.Invoke(LinkedBoard, this);
        }
    }
}
