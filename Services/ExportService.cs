
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Workbase
{
    public static class ExportService
    {
        public static void ExportJson(string filePath)
        {
            // 1. Ambil seluruh data dari database.
            var projects = DatabaseService.GetProjects();
            var tasks = DatabaseService.GetAllTasks();
            var statuses = DatabaseService.GetAllTaskStatuses();

            // 2. Susun data ke dalam model export.
            var exportData = new WorkbaseExport
            {
                Application = "Workbase",
                Version = "0.1.0",
                ExportedAt = DateTime.Now,

                Projects = projects.Select(project =>
                    new ProjectExport
                    {
                        Id = project.Id,
                        Name = project.Name,
                        Description = project.Description,
                        Deadline = project.Deadline,
                        Status = project.Status,

                        TaskStatuses = statuses
                            .Where(status =>
                                status.ProjectId == project.Id)
                            .Select(status =>
                                new ProjectTaskStatusExport
                                {
                                    Id = status.Id,
                                    Name = status.Name,
                                    SortOrder = status.SortOrder,
                                    IsCompleted = status.IsCompleted
                                })
                            .ToList(),

                        Tasks = tasks
                            .Where(task =>
                                task.ProjectId == project.Id)
                            .Select(task =>
                                new ProjectTaskExport
                                {
                                    Id = task.Id,
                                    Title = task.Title,
                                    Description = task.Description,
                                    StatusId = task.StatusId,
                                    Status = task.Status,
                                    Priority = task.Priority,
                                    Deadline = task.Deadline
                                })
                            .ToList()
                    })
                    .ToList()
            };

            // 3. Konfigurasi JSON.
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            // 4. Serialisasi objek menjadi JSON.
            string json = JsonSerializer.Serialize(
                exportData,
                options
            );

            // 5. Simpan JSON ke file.
            File.WriteAllText(filePath, json);
        }
    }
}
