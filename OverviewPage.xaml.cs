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
            LoadStatistics();
            LoadRecentTasks();
        }

        private void LoadStatistics()
        {
            var statistics = DatabaseService.GetTaskStatistics();

            TotalTasksText.Text =
                statistics.TotalTasks.ToString();

            InProgressTasksText.Text =
                statistics.InProgressTasks.ToString();

            CompletedTasksText.Text =
                statistics.CompletedTasks.ToString();
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
                    dialog.ProjectStatus);

                LoadProjects();
            }
        }

        private void ProjectCard_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not ProjectCard card)
                return;

            card.OpenRequested -= ProjectCard_OpenRequested;
            card.EditRequested -= ProjectCard_EditRequested;
            card.DeleteRequested -= ProjectCard_DeleteRequested;

            card.OpenRequested += ProjectCard_OpenRequested;
            card.EditRequested += ProjectCard_EditRequested;
            card.DeleteRequested += ProjectCard_DeleteRequested;
        }

        private void ProjectCard_OpenRequested(Project project)
        {
            NavigationService?.Navigate(new ProjectDetailPage(project));
        }

        private void ProjectCard_EditRequested(Project project)
        {
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
                    dialog.ProjectStatus);

                LoadProjects();
            }
        }

        private void ProjectCard_DeleteRequested(Project project)
        {
            var result = MessageBox.Show(
                $"Delete project '{project.Name}' and all its tasks?\n\n" +
                "This action cannot be undone.",
                "Delete Project",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            DatabaseService.DeleteProject(project.Id);
            LoadProjects();
        }

        private void LoadRecentTasks()
        {
            RecentTasksItemsControl.ItemsSource =
                DatabaseService.GetRecentTasks();
        }
    }
}