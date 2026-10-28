
using System;
using System.Windows;
using System.Windows.Controls;

namespace Workbase
{
    public partial class ProjectCard : UserControl
    {
        public event Action<Project>? OpenRequested;
        public event Action<Project>? EditRequested;
        public event Action<Project>? DeleteRequested;

        public ProjectCard()
        {
            InitializeComponent();
        }

        private void OpenProject_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DataContext is Project project)
                OpenRequested?.Invoke(project);
        }

        private void EditProject_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DataContext is Project project)
                EditRequested?.Invoke(project);
        }

        private void DeleteProject_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DataContext is Project project)
                DeleteRequested?.Invoke(project);
        }
    }
}
