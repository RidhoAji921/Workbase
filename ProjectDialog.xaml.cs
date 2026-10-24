
using System.Windows;

namespace Workbase
{
    public partial class ProjectDialog : Window
    {
        private readonly int? projectId;

        public string ProjectName => NameTextBox.Text.Trim();

        public string ProjectDescription =>
            DescriptionTextBox.Text.Trim();

        public ProjectDialog(Project? project = null)
        {
            InitializeComponent();

            if (project != null)
            {
                projectId = project.Id;
                DialogTitle.Text = "Edit Project";
                NameTextBox.Text = project.Name;
                DescriptionTextBox.Text = project.Description;
            }
            else
            {
                DialogTitle.Text = "Create Project";
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
