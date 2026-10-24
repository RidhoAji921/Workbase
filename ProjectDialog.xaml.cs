
using System;
using System.Windows;
using System.Windows.Controls;

namespace Workbase
{
    public partial class ProjectDialog : Window
    {
        public string ProjectName => NameTextBox.Text.Trim();

        public string ProjectDescription =>
            DescriptionTextBox.Text.Trim();

        public DateTime? ProjectDeadline =>
            DeadlineDatePicker.SelectedDate;

        public string ProjectStatus =>
            (StatusComboBox.SelectedItem as ComboBoxItem)
                ?.Content?.ToString() ?? "Planned";

        public ProjectDialog(Project? project = null)
        {
            InitializeComponent();

            if (project != null)
            {
                DialogTitle.Text = "Edit Project";

                NameTextBox.Text = project.Name;
                DescriptionTextBox.Text = project.Description;
                DeadlineDatePicker.SelectedDate = project.Deadline;

                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Content?.ToString() == project.Status)
                    {
                        StatusComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                DialogTitle.Text = "Create Project";
                StatusComboBox.SelectedIndex = 0;
            }

            Loaded += (_, _) => NameTextBox.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                ErrorText.Text = "Project name cannot be empty.";
                NameTextBox.Focus();
                return;
            }

            DialogResult = true;
        }
    }
}
