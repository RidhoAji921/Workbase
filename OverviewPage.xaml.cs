using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Workbase
{
    public partial class OverviewPage : Page
    {
        private readonly ObservableCollection<Project> projects = new();

        public OverviewPage()
        {
            InitializeComponent();

            ProjectsItemsControl.ItemsSource = projects;

            LoadProjects();
        }

        private void LoadProjects()
        {
            projects.Clear();

            foreach (var project in DatabaseService.GetProjects())
            {
                projects.Add(project);
            }
        }

        private void AddProject_Click(
            object sender,
            RoutedEventArgs e)
        {
            var dialog = new ProjectDialog
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                DatabaseService.AddProject(
                    dialog.ProjectName,
                    dialog.ProjectDescription,
                    dialog.ProjectDeadline,
                    dialog.ProjectStatus
                );

                LoadProjects();
            }
        }

        private void EditProject_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element ||
                element.DataContext is not Project project)
            {
                return;
            }

            var dialog = new ProjectDialog(project)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                DatabaseService.UpdateProject(
                    project.Id,
                    dialog.ProjectName,
                    dialog.ProjectDescription,
                    dialog.ProjectDeadline,
                    dialog.ProjectStatus
                );

                LoadProjects();
            }
        }

        private void DeleteProject_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element ||
                element.DataContext is not Project project)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Delete project '{project.Name}' and all its tasks?\n\n" +
                "This action cannot be undone.",
                "Delete Project",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            DatabaseService.DeleteProject(project.Id);
            LoadProjects();
        }
    }
}