
using System;
using System.Collections.Generic;

namespace Workbase
{
    public class WorkbaseExport
    {
        public string Application { get; set; } = "Workbase";
        public string Version { get; set; } = "0.1.0";
        public DateTime ExportedAt { get; set; } = DateTime.Now;

        public List<ProjectExport> Projects { get; set; } = new();
    }

    public class ProjectExport
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? Deadline { get; set; }
        public string Status { get; set; } = "";

        public List<ProjectTaskStatusExport> TaskStatuses { get; set; } = new();
        public List<ProjectTaskExport> Tasks { get; set; } = new();
    }

    public class ProjectTaskStatusExport
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int SortOrder { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class ProjectTaskExport
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int StatusId { get; set; }
        public string Status { get; set; } = "";
        public string Priority { get; set; } = "";
        public DateTime? Deadline { get; set; }
    }
}
