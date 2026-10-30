using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Workbase
{
    public partial class MainWindow : Window
    {
        private readonly Brush activeBackground =
            new SolidColorBrush(Color.FromRgb(239, 238, 255));

        private readonly Brush activeForeground =
            new SolidColorBrush(Color.FromRgb(79, 70, 229));

        private readonly Brush inactiveForeground =
            new SolidColorBrush(Color.FromRgb(119, 126, 145));

        public MainWindow()
        {
            InitializeComponent();

            DatabaseService.Initialize();

            NavigateToOverview();
        }

        private void NavigateToOverview()
        {
            MainFrame.Navigate(new OverviewPage());
            SetActiveNavigation(OverviewButton);
        }

        private void MyTasks_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MyTasksPage());
            SetActiveNavigation(MyTaskButton);
        }

        private void SettingsButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            MainFrame.Navigate(new SettingsPage()); 
            SetActiveNavigation(SettingsButton);
        }

        private void Overview_Click(
            object sender,
            RoutedEventArgs e)
        {
            NavigateToOverview();
        }

        private void Projects_Click(
            object sender,
            RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProjectsPage());
            SetActiveNavigation(ProjectsButton);
        }

        private void SetActiveNavigation(Button activeButton)
        {
            foreach (var button in new[] {
                OverviewButton,
                MyTaskButton,
                ProjectsButton,
                SettingsButton
            })
            {
                bool isActive = button == activeButton;

                button.Background = isActive
                    ? activeBackground
                    : Brushes.Transparent;

                button.Foreground = isActive
                    ? activeForeground
                    : inactiveForeground;

                button.FontWeight = isActive
                    ? FontWeights.SemiBold
                    : FontWeights.Normal;
            }
        }
    }
}