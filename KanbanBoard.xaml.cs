
using System.Collections.Generic;
using System.Windows.Controls;

namespace Workbase
{
    public partial class KanbanBoard : UserControl
    {
        public KanbanBoard()
        {
            InitializeComponent();
        }

        public void SetColumns(List<KanbanColumn> columns)
        {
            ColumnsItemsControl.ItemsSource = columns;
        }
    }
}
