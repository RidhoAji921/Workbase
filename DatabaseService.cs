
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace Workbase
{
    public static class DatabaseService
    {
        private static readonly string DatabasePath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Workbase",
                "workbase.db"
            );

        private static SqliteConnection OpenConnection()
        {
            var connection =
                new SqliteConnection($"Data Source={DatabasePath}");

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();

            return connection;
        }

        public static void Initialize()
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(DatabasePath)!
            );

            using var connection = OpenConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Projects (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL DEFAULT ''
                );

                CREATE TABLE IF NOT EXISTS Tasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    Status TEXT NOT NULL DEFAULT 'Todo',
                    ProjectId INTEGER REFERENCES Projects(Id)
                        ON DELETE CASCADE
                );
            ";

            command.ExecuteNonQuery();

            // Migrasi database lama yang sudah memiliki tabel Tasks.
            bool hasProjectId = false;

            using (var check = connection.CreateCommand())
            {
                check.CommandText = "PRAGMA table_info(Tasks);";

                using var reader = check.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.GetString(1) == "ProjectId")
                    {
                        hasProjectId = true;
                        break;
                    }
                }
            }

            if (!hasProjectId)
            {
                using var migration = connection.CreateCommand();

                migration.CommandText = @"
                    ALTER TABLE Tasks
                    ADD COLUMN ProjectId INTEGER
                    REFERENCES Projects(Id) ON DELETE CASCADE;
                ";

                migration.ExecuteNonQuery();
            }
        }

        public static List<Project> GetProjects()
        {
            var projects = new List<Project>();

            using var connection = OpenConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT Id, Name, Description
                FROM Projects
                ORDER BY Id DESC;
            ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                projects.Add(new Project
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2)
                });
            }

            return projects;
        }

        public static void AddProject(string name, string description)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                INSERT INTO Projects (Name, Description)
                VALUES ($name, $description);
            ";

            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$description", description);

            command.ExecuteNonQuery();
        }

        public static void UpdateProject(
            int id,
            string name,
            string description)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                UPDATE Projects
                SET Name = $name, Description = $description
                WHERE Id = $id;
            ";

            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$description", description);

            command.ExecuteNonQuery();
        }

        public static void DeleteProject(int id)
        {
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();

            using var command = connection.CreateCommand();

            command.Transaction = transaction;
            command.CommandText = @"
                DELETE FROM Projects
                WHERE Id = $id;
            ";

            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();

            transaction.Commit();
        }


        public static List<ProjectTask> GetTasksForProject(int projectId)
        {
            var tasks = new List<ProjectTask>();

            using var connection = OpenConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
        SELECT Id, ProjectId, Title, Description, Status
        FROM Tasks
        WHERE ProjectId = $projectId
        ORDER BY Id DESC;
    ";

            command.Parameters.AddWithValue("$projectId", projectId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(new ProjectTask
                {
                    Id = reader.GetInt32(0),
                    ProjectId = reader.GetInt32(1),
                    Title = reader.GetString(2),
                    Description = reader.IsDBNull(3)
                        ? ""
                        : reader.GetString(3),
                    Status = reader.GetString(4)
                });
            }

            return tasks;
        }

        public static void AddTask(
            int projectId,
            string title,
            string description,
            string status)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
        INSERT INTO Tasks (ProjectId, Title, Description, Status)
        VALUES ($projectId, $title, $description, $status);
    ";

            command.Parameters.AddWithValue("$projectId", projectId);
            command.Parameters.AddWithValue("$title", title);
            command.Parameters.AddWithValue("$description", description);
            command.Parameters.AddWithValue("$status", status);

            command.ExecuteNonQuery();
        }

        public static void UpdateTask(
            int id,
            string title,
            string description,
            string status)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
        UPDATE Tasks
        SET Title = $title,
            Description = $description,
            Status = $status
        WHERE Id = $id;
    ";

            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$title", title);
            command.Parameters.AddWithValue("$description", description);
            command.Parameters.AddWithValue("$status", status);

            command.ExecuteNonQuery();
        }

        public static void DeleteTask(int id)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
        DELETE FROM Tasks
        WHERE Id = $id;
    ";

            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }

    }
}
