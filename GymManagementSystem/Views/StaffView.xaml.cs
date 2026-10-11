using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GymManagementSystem.Data;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Models;
using GymManagementSystem.Services;
using GymManagementSystem.Views.Dialogs;

namespace GymManagementSystem.Views
{
    public partial class StaffView : UserControl
    {
        private readonly StaffRepository _staffRepo = new();

        public StaffView()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadStaffAccounts();
        }

        public void LoadStaffAccounts()
        {
            if (DgStaff == null) return;
            var accounts = _staffRepo.GetAll();
            DgStaff.ItemsSource = accounts;
            if (TxtFooterCount != null)
            {
                TxtFooterCount.Text = $"Showing {accounts.Count} registered staff account(s)";
            }
        }

        private void BtnAddStaff_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new StaffDialog
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true && dialog.StaffResult != null && !string.IsNullOrEmpty(dialog.PlainPasswordResult))
            {
                _staffRepo.Add(dialog.StaffResult, dialog.PlainPasswordResult);
                LoadStaffAccounts();
                MessageBox.Show($"Staff account '{dialog.StaffResult.Username}' created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnEditStaff_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Staff staff)
            {
                var dialog = new StaffDialog(staff)
                {
                    Owner = Window.GetWindow(this)
                };
                if (dialog.ShowDialog() == true && dialog.StaffResult != null)
                {
                    _staffRepo.Update(dialog.StaffResult, dialog.PlainPasswordResult);
                    LoadStaffAccounts();
                    MessageBox.Show($"Staff account '{dialog.StaffResult.Username}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnDeleteStaff_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Staff staff)
            {
                if (staff.StaffId == AuthService.CurrentStaff?.StaffId)
                {
                    MessageBox.Show("You cannot delete the account you are currently logged in with.", "Cannot Delete Self", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var res = MessageBox.Show($"Are you sure you want to delete staff account '{staff.Username}' ({staff.FullName})?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    _staffRepo.Delete(staff.StaffId);
                    LoadStaffAccounts();
                    MessageBox.Show("Staff account deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "SQLite Database Backup (*.db)|*.db",
                FileName = $"gym_backup_{DateTime.Now:yyyyMMdd_HHmm}.db"
            };

            if (saveDialog.ShowDialog() == true)
            {
                bool success = DatabaseHelper.BackupDatabase(saveDialog.FileName);
                if (success)
                {
                    MessageBox.Show($"Database successfully backed up to:\n{saveDialog.FileName}", "Backup Succeeded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to complete database backup.", "Backup Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
