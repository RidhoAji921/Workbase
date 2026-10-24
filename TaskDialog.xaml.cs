
using System.Windows;
using System.Windows.Controls;

namespace Workbase
{
    public partial class TaskDialog : Window
    {
        public string TaskTitle => TitleTextBox.Text.Trim();

        public string TaskDescription =>
            DescriptionTextBox.Text.Trim();

        public string TaskStatus =>
            (StatusComboBox.SelectedItem as ComboBoxItem)
                ?.Content?.ToString() ?? "Todo";

        public TaskDialog(ProjectTask? task = null)
        {
            InitializeComponent();

            if (task != null)
            {
                DialogTitle.Text = "Edit Task";
                TitleTextBox.Text = task.Title;
                DescriptionTextBox.Text = task.Description;

                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Content?.ToString() == task.Status)
                    {
                        StatusComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                DialogTitle.Text = "Create Task";
                StatusComboBox.SelectedIndex = 0;
            }

            Loaded += (_, _) => TitleTextBox.Focus();
        }

        private void Save_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitle))
            {
                ErrorText.Text = "Task title cannot be empty.";
                TitleTextBox.Focus();
                return;
            }

            if (StatusComboBox.SelectedItem == null)
            {
                ErrorText.Text = "Please select a task status.";
                return;
            }

            DialogResult = true;
        }
    }
}
