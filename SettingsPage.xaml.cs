
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Workbase
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void ExportButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Export Workbase Data",
                FileName = "workbase-export",
                DefaultExt = ".json",
                Filter = "JSON Files (*.json)|*.json"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                ExportService.ExportJson(dialog.FileName);

                MessageBox.Show(
                    "Your Workbase data has been exported successfully.",
                    "Export Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to export data.\n\n{ex.Message}",
                    "Export Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Import Workbase Data",
                Filter = "JSON Files (*.json)|*.json",
                DefaultExt = ".json",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                // 1. Read and validate JSON
                var importData = ImportService.ReadJson(dialog.FileName);

                // 2. Ask for confirmation
                var confirmation = MessageBox.Show(
                    "Importing this file will replace ALL existing Workbase data.\n\n" +
                    "A backup of your current database will be created automatically.\n\n" +
                    "Do you want to continue?",
                    "Confirm Data Import",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmation != MessageBoxResult.Yes)
                    return;

                // 3. Create backup
                string backupPath = BackupService.CreateAutomaticBackup();

                // 4. Replace database contents
                DatabaseService.ImportData(importData);

                MessageBox.Show(
                    $"Your data has been imported successfully.\n\n" +
                    $"Backup saved at:\n{backupPath}",
                    "Import Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Refresh the current page
                NavigationService?.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to import data.\n\n{ex.Message}",
                    "Import Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

    }
}
