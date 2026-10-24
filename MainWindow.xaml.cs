
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Workbase
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<Project> projects = new();

        public MainWindow()
        {
            InitializeComponent();

            ProjectsItemsControl.ItemsSource = projects;

            DatabaseService.Initialize();
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
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                DatabaseService.AddProject(
                    dialog.ProjectName,
                    dialog.ProjectDescription
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
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                DatabaseService.UpdateProject(
                    project.Id,
                    dialog.ProjectName,
                    dialog.ProjectDescription
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

        private void Overview_Click(
    object sender,
    RoutedEventArgs e)
        {
            // Tampilkan dashboard.
            MainFrame.Visibility = Visibility.Collapsed;
            DashboardContent.Visibility = Visibility.Visible;

            // Perbarui tampilan menu aktif.
            OverviewButton.Background =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(239, 238, 255));

            OverviewButton.Foreground =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(79, 70, 229));

            OverviewButton.FontWeight = FontWeights.SemiBold;

            ProjectsButton.ClearValue(Button.BackgroundProperty);
            ProjectsButton.Foreground =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(119, 126, 145));

            ProjectsButton.FontWeight = FontWeights.Normal;
        }

        private void Projects_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Tampilkan halaman Projects.
            MainFrame.Navigate(new ProjectsPage());

            DashboardContent.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;

            // Perbarui tampilan menu aktif.
            ProjectsButton.Background =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(239, 238, 255));

            ProjectsButton.Foreground =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(79, 70, 229));

            ProjectsButton.FontWeight = FontWeights.SemiBold;

            OverviewButton.ClearValue(Button.BackgroundProperty);
            OverviewButton.Foreground =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(119, 126, 145));

            OverviewButton.FontWeight = FontWeights.Normal;
        }
    }
}
