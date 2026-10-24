
using System.Windows;
using System.Windows.Controls;

namespace Workbase
{
    public partial class ProjectCard : UserControl
    {
        public ProjectCard()
        {
            InitializeComponent();
        }

        private void OpenProject_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DataContext is not Project project)
                return;

            var page = FindParentPage();

            page?.OpenProject(project);
        }

        private void EditProject_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DataContext is not Project project)
                return;

            var page = FindParentPage();

            page?.EditProject(project);
        }

        private void DeleteProject_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DataContext is not Project project)
                return;

            var page = FindParentPage();

            page?.DeleteProject(project);
        }

        private ProjectsPage? FindParentPage()
        {
            DependencyObject? current = this;

            while (current != null)
            {
                if (current is ProjectsPage page)
                    return page;

                current =
                    System.Windows.Media.VisualTreeHelper
                        .GetParent(current);
            }

            return null;
        }
    }
}
