
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Workbase
{
    public partial class TaskDialog : Window
    {
        public string TaskTitle => TitleTextBox.Text.Trim();

        public string TaskDescription =>
            DescriptionTextBox.Text.Trim();

        public int TaskStatusId =>
            StatusComboBox.SelectedValue is int id ? id : 0;

        public string TaskPriority =>
            (PriorityComboBox.SelectedItem as ComboBoxItem)
                ?.Content?.ToString() ?? "Medium";

        public DateTime? TaskDeadline =>
            DeadlineDatePicker.SelectedDate;

        public TaskDialog(int projectId, ProjectTask? task = null)
        {
            InitializeComponent();

            // Load workflow milik proyek yang sedang dibuka.
            StatusComboBox.ItemsSource =
                DatabaseService.GetTaskStatusesForProject(projectId);

            if (task != null)
            {
                DialogTitle.Text = "Edit Task";
                TitleTextBox.Text = task.Title;
                DescriptionTextBox.Text = task.Description;

                StatusComboBox.SelectedValue = task.StatusId;

                foreach (ComboBoxItem item in PriorityComboBox.Items)
                {
                    if (item.Content?.ToString() == task.Priority)
                    {
                        PriorityComboBox.SelectedItem = item;
                        break;
                    }
                }

                DeadlineDatePicker.SelectedDate = task.Deadline;
            }
            else
            {
                DialogTitle.Text = "Create Task";

                if (StatusComboBox.Items.Count > 0)
                    StatusComboBox.SelectedIndex = 0;

                PriorityComboBox.SelectedIndex = 1;
            }

            Loaded += (_, _) => TitleTextBox.Focus();
        }

        private void ClearDeadline_Click(
            object sender,
            RoutedEventArgs e)
        {
            DeadlineDatePicker.SelectedDate = null;
        }

        private void Save_Click(
            object sender,
            RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (string.IsNullOrWhiteSpace(TaskTitle))
            {
                ErrorText.Text = "Task title cannot be empty.";
                TitleTextBox.Focus();
                return;
            }

            if (StatusComboBox.SelectedItem == null)
            {
                ErrorText.Text =
                    "This project has no available task status.";

                return;
            }

            if (PriorityComboBox.SelectedItem == null)
            {
                ErrorText.Text = "Please select a priority.";
                return;
            }

            DialogResult = true;
        }
    }
}
