
using System;
using System.IO;

namespace Workbase
{
    public static class BackupService
    {
        public static string CreateAutomaticBackup()
        {
            string backupDirectory = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Workbase",
                "Backups"
            );

            Directory.CreateDirectory(backupDirectory);

            string timestamp = DateTime.Now.ToString(
                "yyyyMMdd-HHmmss");

            string backupPath = Path.Combine(
                backupDirectory,
                $"workbase-backup-{timestamp}.db"
            );

            DatabaseService.CreateBackup(backupPath);

            return backupPath;
        }
    }
}
