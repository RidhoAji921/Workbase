
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Workbase
{
    public partial class ProjectsPage : Page
    {
        private readonly ObservableCollection<Project> projects = new();

        public ProjectsPage()
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

            RefreshProjectList();
        }

        private void RefreshProjectList()
        {
            var keyword = SearchTextBox.Text.Trim();

            var filteredProjects = projects
                .Where(project =>
                    project.Name.Contains(
                        keyword,
                        System.StringComparison.OrdinalIgnoreCase)
                    || project.Description.Contains(
                        keyword,
                        System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            ProjectsItemsControl.ItemsSource = filteredProjects;

            ProjectCountText.Text =
                $"Projects ({filteredProjects.Count})";

            EmptyText.Visibility = filteredProjects.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (ProjectsItemsControl != null)
            {
                RefreshProjectList();
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
                    dialog.ProjectStatus);

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
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            DatabaseService.DeleteProject(project.Id);
            LoadProjects();
        }

        private void OpenProject_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element ||
                element.DataContext is not Project project)
            {
                return;
            }

            NavigationService?.Navigate(
                new ProjectDetailPage(project));
        }


        public void OpenProject(Project project)
        {
            NavigationService?.Navigate(
                new ProjectDetailPage(project));
        }

        public void EditProject(Project project)
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

        public void DeleteProject(Project project)
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

        private void ProjectCard_OpenRequested(Project project)
        {
            OpenProject(project);
        }

        private void ProjectCard_EditRequested(Project project)
        {
            EditProject(project);
        }

        private void ProjectCard_DeleteRequested(Project project)
        {
            DeleteProject(project);
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
    }
}
