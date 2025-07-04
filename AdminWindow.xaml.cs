using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Nova
{
    public partial class AdminWindow : Window
    {
        private string filePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, "Resources", "login_records.txt");
        private List<LoginRecord> loginRecords = new List<LoginRecord>();

        public AdminWindow()
        {
            InitializeComponent();
            LoadLoginRecords();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            Application.Current.Shutdown();
        }

        private void LoadLoginRecords()
        {
            if (File.Exists(filePath))
            {
                loginRecords = ReadLoginRecords(filePath);
                LoginRecordsListView.ItemsSource = loginRecords.OrderByDescending(r => DateTime.Parse(r.LoginTime)).ToList();
            }
            else
            {
                MessageBox.Show("Login records file not found!");
            }
        }

        private List<LoginRecord> ReadLoginRecords(string filePath)
        {
            var records = new List<LoginRecord>();
            var lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i])) // Ignore empty lines
                {
                    if (i + 1 < lines.Length && !string.IsNullOrWhiteSpace(lines[i + 1])) // Ensure logout time exists
                    {
                        string[] parts = lines[i].Split(" - ");
                        if (parts.Length == 2)
                        {
                            records.Add(new LoginRecord
                            {
                                LoginTime = parts[0].Trim(),
                                UserName = parts[1].Trim(),
                                LogoutTime = lines[i + 1].Trim()
                            });
                        }
                        i++; // Skip next line (logout time)
                    }
                }
            }
            return records;
        }

        private void SortByUserName_Click(object sender, RoutedEventArgs e)
        {
            LoginRecordsListView.ItemsSource = loginRecords.OrderBy(r => r.UserName).ToList();
        }

        private void SortByUserNameDesc_Click(object sender, RoutedEventArgs e)
        {
            LoginRecordsListView.ItemsSource = loginRecords.OrderByDescending(r => r.UserName).ToList();
        }

        private void SortByLoginTime_Click(object sender, RoutedEventArgs e)
        {
            LoginRecordsListView.ItemsSource = loginRecords.OrderBy(r => DateTime.Parse(r.LoginTime)).ToList();
        }

        private void SortByLoginTimeDesc_Click(object sender, RoutedEventArgs e)
        {
            LoginRecordsListView.ItemsSource = loginRecords.OrderByDescending(r => DateTime.Parse(r.LoginTime)).ToList();
        }

        private void ExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Do you want to export only today's records?",
                "Export Options",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel) return;

            List<LoginRecord> recordsToExport = loginRecords;
            if (result == MessageBoxResult.Yes)
            {
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                recordsToExport = loginRecords.Where(r => r.LoginTime.StartsWith(today)).ToList();
            }

            if (recordsToExport.Count == 0)
            {
                MessageBox.Show("No records available for export.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = "LoginRecords.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    writer.WriteLine("UserName,LoginTime,LogoutTime");
                    foreach (var record in recordsToExport)
                    {
                        writer.WriteLine($"{record.UserName},{record.LoginTime},{record.LogoutTime}");
                    }
                }
                MessageBox.Show("Login records exported successfully!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void ClearAllRecords_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Do you want to delete all available login records",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    File.WriteAllText(filePath, string.Empty); // Clears File
                    loginRecords.Clear(); // Clears the List of Records
                    LoginRecordsListView.ItemsSource = null; // Updates the UI
                    MessageBox.Show("All login records have been deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


    }

    public class LoginRecord
    {
        public string UserName { get; set; }
        public string LoginTime { get; set; }
        public string LogoutTime { get; set; }
    }
}
