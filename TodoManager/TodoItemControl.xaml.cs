using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TodoManager
{
    public partial class TodoItemControl : UserControl
    {
        public event Action? ItemChanged;

        public event Action<String?, TodoItemControl>? LinkButtonClicked;

        private Point dragStartPoint;

        private bool isEditing;

        /// <summary>
        /// This method gets and sets is editing
        /// </summary>
        public bool IsEditing
        {
            get => isEditing;
            set => isEditing = value;
        }

        private string? title;
        private string? description;
        private DateTime? startDate;
        private DateTime? endDate;
        private string? linkedBoard;

        private readonly StackPanel todoContainer;
        private readonly StackPanel inProgressContainer;
        private readonly StackPanel doneContainer;

        /// <summary>
        /// This method gets and sets title
        /// </summary>
        public string? Title
        {
            get => title;
            set
                {
                    title = value;
                    ItemChanged?.Invoke();
                }
            }

        /// <summary>
        /// This method gets and sets description
        /// </summary>
        public string? Description
        {
            get => description;
            set
                {
                    description = value;
                    ItemChanged?.Invoke();
                }
        }

        /// <summary>
        /// This method gets and sets start date
        /// </summary>
        public DateTime? StartDate
        {
            get => startDate;
            set
                {
                    startDate = value;
                    ItemChanged?.Invoke();
                }
        }

        /// <summary>
        /// This method gets and sets end date
        /// </summary>
        public DateTime? EndDate
        {
            get => endDate;
            set
                {
                    endDate = value;
                    ItemChanged?.Invoke();
                }
        }

        /// <summary>
        /// This method gets and sets linked board
        /// </summary>
        public string? LinkedBoard
        {
            get => linkedBoard;
            set
            {
                linkedBoard = value;
                ItemChanged?.Invoke();
            }                
        }

        /// <summary>
        /// Constructor, Initializes component and sets data context
        /// </summary>
        public TodoItemControl(StackPanel todoContainer, StackPanel inProgressContainer, StackPanel doneContainer)
        {
            InitializeComponent();
            DataContext = this;

            this.todoContainer = todoContainer;
            this.inProgressContainer = inProgressContainer;
            this.doneContainer = doneContainer;
        }

        /// <summary>
        /// This method records the start point on mouse down for the drag operation
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                dragStartPoint = e.GetPosition(this);
            }
        }

        /// <summary>
        /// This method handles setting is editing to true when an editable part of the item gains focus
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private new void GotFocus(object sender, RoutedEventArgs e)
        {
            IsEditing = true; // User is editing
        }

        /// <summary>
        /// This method handles setting is editing to true when an editable part of the item loses focus
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private new void LostFocus(object sender, RoutedEventArgs e)
        {
            IsEditing = false; // User finished editing
        }

        /// <summary>
        /// This method handles starting the drag drop operation when the mouse moves
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            // If the user is editing prevent drag drop operation
            if (IsEditing) return;

            // Ensure the user is also holding down the left mouse button
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


        /// <summary>
        /// This method handles moving the item to the container on its left when the button is clicked
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void MoveLeft_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent == inProgressContainer)
            {
                // Move to Todo
                inProgressContainer.Children.Remove(this);
                todoContainer!.Children.Add(this);
                // Notify listeners
                ItemChanged?.Invoke();
            }
            else if (this.Parent == doneContainer)
            {
                // Move to In Progress
                doneContainer.Children.Remove(this);
                inProgressContainer!.Children.Add(this);
                // Notify listeners
                ItemChanged?.Invoke();
            }
        }

        /// <summary>
        /// This method handles moving the item to the container on its right when the button is clicked
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void MoveRight_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent == todoContainer)
            {
                // Move to In Progress
                todoContainer.Children.Remove(this);
                inProgressContainer!.Children.Add(this);
                // Notify listeners
                ItemChanged?.Invoke();
            }
            else if (this.Parent == inProgressContainer)
            {
                // Move to Done
                inProgressContainer.Children.Remove(this);
                doneContainer!.Children.Add(this);
                // Notify listeners
                ItemChanged?.Invoke();
            }
        }

        /// <summary>
        /// This method handles moving the item up in the container it is currently in
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (Parent is StackPanel container)
            {
                int currentIndex = container.Children.IndexOf(this);
                if (currentIndex > 0)
                {
                    container.Children.Remove(this);
                    container.Children.Insert(currentIndex - 1, this);
                    // Notify listeners
                    ItemChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// This method handles moving the item down in the container it is currently in
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
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
                    ItemChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// This method handles deleting an item
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (Parent is StackPanel container)
            {
                container.Children.Remove(this);
                // Notify listeners
                ItemChanged?.Invoke();
            }
        }

        /// <summary>
        /// This method handles calling the link method on the main window class when the button is clicked
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args</param>
        private void Link_Click(object sender, RoutedEventArgs e)
        {
            LinkButtonClicked?.Invoke(linkedBoard, this);
        }
    }
}
