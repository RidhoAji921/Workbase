
namespace Workbase
{
    public class ProjectTaskStatus
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string Name { get; set; } = "";

        public int SortOrder { get; set; }

        public bool IsCompleted { get; set; }
    }
}
