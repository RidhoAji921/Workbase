
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

            // 1. Buat semua tabel terlebih dahulu.
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

        CREATE TABLE IF NOT EXISTS ProjectTaskStatuses (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ProjectId INTEGER NOT NULL
                REFERENCES Projects(Id) ON DELETE CASCADE,
            Name TEXT NOT NULL,
            SortOrder INTEGER NOT NULL DEFAULT 0,
            IsCompleted INTEGER NOT NULL DEFAULT 0,
            UNIQUE(ProjectId, Name)
        );
    ";

            command.ExecuteNonQuery();

            // 2. Migrasi database lama:
            // tambahkan ProjectId jika belum tersedia.
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

            // 3. Pastikan kolom tambahan Projects tersedia.
            EnsureProjectColumnExists(
                connection,
                "Deadline",
                "TEXT NULL"
            );

            EnsureProjectColumnExists(
                connection,
                "Status",
                "TEXT NOT NULL DEFAULT 'Planned'"
            );

            // 4. Buat workflow default untuk proyek yang belum memilikinya.
            EnsureDefaultTaskStatuses(connection);
        }


        private static void EnsureProjectColumnExists(
            SqliteConnection connection,
            string columnName,
            string columnDefinition)
                {
                    bool exists = false;

                    using (var check = connection.CreateCommand())
                    {
                        check.CommandText = "PRAGMA table_info(Projects);";

                        using var reader = check.ExecuteReader();

                        while (reader.Read())
                        {
                            if (reader.GetString(1) == columnName)
                            {
                                exists = true;
                                break;
                            }
                        }
                    }

                    if (exists)
                        return;

                    using var alter = connection.CreateCommand();

                    alter.CommandText = $@"
                ALTER TABLE Projects
                ADD COLUMN {columnName} {columnDefinition};
            ";

            alter.ExecuteNonQuery();
        }

        public static List<Project> GetProjects()
        {
            var projects = new List<Project>();

            using var connection = OpenConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
        SELECT Id, Name, Description, Deadline, Status
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
                    Description = reader.GetString(2),

                    Deadline = reader.IsDBNull(3)
                        ? null
                        : DateTime.Parse(reader.GetString(3)),

                    Status = reader.GetString(4)
                });
            }

            return projects;
        }


        public static void AddProject(
            string name,
            string description,
            DateTime? deadline,
            string status)
        {
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();

            // 1. Simpan proyek.
            long projectId;

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;

                command.CommandText = @"
            INSERT INTO Projects (
                Name,
                Description,
                Deadline,
                Status
            )
            VALUES (
                $name,
                $description,
                $deadline,
                $status
            );

            SELECT last_insert_rowid();
        ";

                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$description", description);

                command.Parameters.AddWithValue(
                    "$deadline",
                    deadline.HasValue
                        ? deadline.Value.ToString("yyyy-MM-dd")
                        : DBNull.Value
                );

                command.Parameters.AddWithValue("$status", status);

                projectId = (long)command.ExecuteScalar()!;
            }

            // 2. Buat workflow default untuk proyek baru.
            using (var statusCommand = connection.CreateCommand())
            {
                statusCommand.Transaction = transaction;

                statusCommand.CommandText = @"
            INSERT INTO ProjectTaskStatuses (
                ProjectId,
                Name,
                SortOrder,
                IsCompleted
            )
            VALUES
                ($projectId, 'Todo', 0, 0),
                ($projectId, 'In Progress', 1, 0),
                ($projectId, 'Completed', 2, 1);
        ";

                statusCommand.Parameters.AddWithValue(
                    "$projectId",
                    projectId
                );

                statusCommand.ExecuteNonQuery();
            }

            // 3. Simpan kedua perubahan sekaligus.
            transaction.Commit();
        }


        public static void UpdateProject(
            int id,
            string name,
            string description,
            DateTime? deadline,
            string status)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
        UPDATE Projects
        SET Name = $name,
            Description = $description,
            Deadline = $deadline,
            Status = $status
        WHERE Id = $id;
    ";

            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$description", description);

            command.Parameters.AddWithValue(
                "$deadline",
                deadline.HasValue
                    ? deadline.Value.ToString("yyyy-MM-dd")
                    : DBNull.Value
            );

            command.Parameters.AddWithValue("$status", status);

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


        private static void EnsureDefaultTaskStatuses(
            SqliteConnection connection)
        {
            using var command = connection.CreateCommand();

            command.CommandText = @"
        INSERT INTO ProjectTaskStatuses
            (ProjectId, Name, SortOrder, IsCompleted)
        SELECT p.Id, defaults.Name,
               defaults.SortOrder, defaults.IsCompleted
        FROM Projects p
        CROSS JOIN (
            SELECT 'Todo' AS Name, 0 AS SortOrder, 0 AS IsCompleted
            UNION ALL
            SELECT 'In Progress', 1, 0
            UNION ALL
            SELECT 'Completed', 2, 1
        ) AS defaults
        WHERE NOT EXISTS (
            SELECT 1
            FROM ProjectTaskStatuses s
            WHERE s.ProjectId = p.Id
        );
    ";

            command.ExecuteNonQuery();
        }

        public static List<ProjectTaskStatus> GetTaskStatusesForProject(
            int projectId)
        {
            var statuses = new List<ProjectTaskStatus>();

            using var connection = OpenConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT Id, ProjectId, Name, SortOrder, IsCompleted
                FROM ProjectTaskStatuses
                WHERE ProjectId = $projectId
                ORDER BY SortOrder, Id;
            ";

            command.Parameters.AddWithValue("$projectId", projectId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                statuses.Add(new ProjectTaskStatus
                {
                    Id = reader.GetInt32(0),
                    ProjectId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    SortOrder = reader.GetInt32(3),
                    IsCompleted = reader.GetInt32(4) != 0
                });
            }

            return statuses;
        }

    }
}
