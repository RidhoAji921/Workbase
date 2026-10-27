
namespace Workbase
{
    public class ProjectTask
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public int StatusId { get; set; }

        // Nama status untuk ditampilkan di UI.
        public string Status { get; set; } = "";

        public string Priority { get; set; } = "Medium";

        public DateTime? Deadline { get; set; }
    }
}
