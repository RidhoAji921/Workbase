using System.Collections.Generic;

namespace Workbase
{
    public class KanbanColumn
    {
        public int StatusId { get; set; }
        public int ProjectId { get; set; }

        public string Name { get; set; } = "";

        public List<ProjectTask> Tasks { get; set; } = new();
    }
}