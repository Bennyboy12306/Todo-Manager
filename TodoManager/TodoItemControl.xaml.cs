using System.Windows;
using System.Windows.Controls;

namespace TodoManager
{
    public partial class TodoItemControl : UserControl
    {

        // Properties to track the parent containers
        public StackPanel TodoContainer { get; set; }
        public StackPanel InProgressContainer { get; set; }
        public StackPanel DoneContainer { get; set; }

        public TodoItemControl()
        {
            InitializeComponent();
        }

        // Properties for Title and Description
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


        public void UpdateButtonStates()
        {
            if (this.Parent == TodoContainer)
            {
                // Cannot move left from Todo
                MoveLeftButton.IsEnabled = false;
                MoveRightButton.IsEnabled = true;
            }
            else if (this.Parent == InProgressContainer)
            {
                // Can move both left and right from In Progress
                MoveLeftButton.IsEnabled = true;
                MoveRightButton.IsEnabled = true;
            }
            else if (this.Parent == DoneContainer)
            {
                // Cannot move right from Done
                MoveLeftButton.IsEnabled = true;
                MoveRightButton.IsEnabled = false;
            }
        }
    }
}