
using System;
using System.IO;
using System.Text.Json;

namespace Workbase
{
    public static class ImportService
    {
        public static WorkbaseExport ReadJson(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The selected file could not be found.");
            }

            string json = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidDataException(
                    "The selected file is empty.");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var importData =
                JsonSerializer.Deserialize<WorkbaseExport>(json, options);

            if (importData == null)
            {
                throw new InvalidDataException(
                    "The JSON file could not be read.");
            }

            if (importData.Application != "Workbase")
            {
                throw new InvalidDataException(
                    "This file is not a Workbase export file.");
            }

            if (importData.Projects == null)
            {
                throw new InvalidDataException(
                    "The export file does not contain project data.");
            }

            ValidateData(importData);
            return importData;
        }


        private static void ValidateData(WorkbaseExport data)
        {
            var projectIds = new HashSet<int>();

            foreach (var project in data.Projects)
            {
                if (project.Id <= 0 || !projectIds.Add(project.Id))
                {
                    throw new InvalidDataException(
                        "The export contains an invalid or duplicate project ID.");
                }

                if (string.IsNullOrWhiteSpace(project.Name))
                {
                    throw new InvalidDataException(
                        "A project is missing its name.");
                }

                if (project.TaskStatuses == null || project.Tasks == null)
                {
                    throw new InvalidDataException(
                        "A project contains invalid task or status data.");
                }

                var statusIds = new HashSet<int>();

                foreach (var status in project.TaskStatuses)
                {
                    if (status.Id <= 0 || !statusIds.Add(status.Id))
                    {
                        throw new InvalidDataException(
                            $"Project '{project.Name}' contains an invalid or duplicate status ID.");
                    }

                    if (string.IsNullOrWhiteSpace(status.Name))
                    {
                        throw new InvalidDataException(
                            $"Project '{project.Name}' contains a status without a name.");
                    }
                }

                var taskIds = new HashSet<int>();

                foreach (var task in project.Tasks)
                {
                    if (task.Id <= 0 || !taskIds.Add(task.Id))
                    {
                        throw new InvalidDataException(
                            $"Project '{project.Name}' contains an invalid or duplicate task ID.");
                    }

                    if (string.IsNullOrWhiteSpace(task.Title))
                    {
                        throw new InvalidDataException(
                            $"Project '{project.Name}' contains a task without a title.");
                    }

                    if (!statusIds.Contains(task.StatusId))
                    {
                        throw new InvalidDataException(
                            $"Task '{task.Title}' references a status that does not exist in its project.");
                    }
                }
            }
        }

    }
}
