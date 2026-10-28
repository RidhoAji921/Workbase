using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Workbase
{
    /// <summary>
    /// Interaction logic for MyTasksPage.xaml
    /// </summary>
    public partial class MyTasksPage : Page
    {
        public MyTasksPage()
        {
            InitializeComponent();
            LoadTasks();
        }

        private void LoadTasks()
        {
            var tasks = DatabaseService.GetAllTasks();
            var statuses = DatabaseService.GetAllTaskStatuses();

            var columns = statuses
                .GroupBy(status => status.Name)
                .Select(group =>
                {
                    var firstStatus = group
                        .OrderBy(status => status.SortOrder)
                        .First();

                    var statusIds = group
                        .Select(status => status.Id)
                        .ToHashSet();

                    return new KanbanColumn
                    {
                        StatusId = firstStatus.Id,
                        ProjectId = 0,
                        Name = group.Key,
                        Tasks = tasks
                            .Where(task => statusIds.Contains(task.StatusId))
                            .ToList()
                    };
                })
                .OrderBy(column => statuses
                    .Where(status => status.Name == column.Name)
                    .Min(status => status.SortOrder))
                .ToList();

            TasksKanbanBoard.SetColumns(columns);
        }
    }
}
