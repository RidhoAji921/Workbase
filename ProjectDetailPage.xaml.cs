
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Workbase
{
    public partial class ProjectDetailPage : Page
    {
        private readonly Project project;
        private readonly ObservableCollection<ProjectTask> tasks = new();

        public ProjectDetailPage(Project project)
        {
            InitializeComponent();

            this.project = project;

            ProjectNameText.Text = project.Name;

            ProjectDescriptionText.Text =
                string.IsNullOrWhiteSpace(project.Description)
                    ? "No description provided."
                    : project.Description;

            TasksItemsControl.ItemsSource = tasks;

            LoadTasks();
        }

        private void LoadTasks()
        {
            tasks.Clear();

            foreach (var task in DatabaseService.GetTasksForProject(project.Id))
            {
                tasks.Add(task);
            }

            TaskCountText.Text =
                $"{tasks.Count} {(tasks.Count == 1 ? "task" : "tasks")}";

            EmptyTasksText.Visibility = tasks.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void AddTask_Click(
            object sender,
            RoutedEventArgs e)
        {
            var dialog = new TaskDialog
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                DatabaseService.AddTask(
                    project.Id,
                    dialog.TaskTitle,
                    dialog.TaskDescription,
                    dialog.TaskStatus);

                LoadTasks();
            }
        }

        private void EditTask_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element ||
                element.DataContext is not ProjectTask task)
            {
                return;
            }

            var dialog = new TaskDialog(task)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                DatabaseService.UpdateTask(
                    task.Id,
                    dialog.TaskTitle,
                    dialog.TaskDescription,
                    dialog.TaskStatus);

                LoadTasks();
            }
        }

        private void DeleteTask_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element ||
                element.DataContext is not ProjectTask task)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Delete task '{task.Title}'?",
                "Delete Task",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            DatabaseService.DeleteTask(task.Id);
            LoadTasks();
        }

        private void BackToProjects_Click(
            object sender,
            RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProjectsPage());
        }
    }
}
