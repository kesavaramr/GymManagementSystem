using System;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Models;

namespace GymManagementSystem.Views.Dialogs
{
    public partial class StaffDialog : Window
    {
        private readonly StaffRepository _staffRepo = new();
        private readonly Staff? _editingStaff;
        public Staff? StaffResult { get; private set; }
        public string? PlainPasswordResult { get; private set; }

        public StaffDialog(Staff? existingStaff = null)
        {
            InitializeComponent();
            _editingStaff = existingStaff;

            if (_editingStaff != null)
            {
                TxtTitle.Text = "Edit Staff Account";
                BtnSave.Content = "Update Account";
                TxtFullName.Text = _editingStaff.FullName;
                TxtUsername.Text = _editingStaff.Username;
                TxtContact.Text = _editingStaff.Contact;

                LblPassword.Text = "New Password (leave blank to keep current)";
                LblConfirmPassword.Text = "Confirm New Password";

                foreach (ComboBoxItem item in CmbRole.Items)
                {
                    if (item.Content.ToString() == _editingStaff.Role)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var fullName = TxtFullName.Text.Trim();
            var username = TxtUsername.Text.Trim();
            var role = (CmbRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Staff";
            var contact = TxtContact.Text.Trim();
            var password = TxtPassword.Password;
            var confirmPassword = TxtConfirmPassword.Password;

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowError("Please enter full name.");
                return;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Please enter username.");
                return;
            }

            if (_staffRepo.UsernameExists(username, _editingStaff?.StaffId ?? 0))
            {
                ShowError($"The username '{username}' is already taken. Please choose another.");
                return;
            }

            if (_editingStaff == null)
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Please enter a password.");
                    return;
                }
                if (password.Length < 4)
                {
                    ShowError("Password must be at least 4 characters.");
                    return;
                }
                if (password != confirmPassword)
                {
                    ShowError("Passwords do not match.");
                    return;
                }
                PlainPasswordResult = password;
            }
            else
            {
                if (!string.IsNullOrEmpty(password))
                {
                    if (password.Length < 4)
                    {
                        ShowError("Password must be at least 4 characters.");
                        return;
                    }
                    if (password != confirmPassword)
                    {
                        ShowError("Passwords do not match.");
                        return;
                    }
                    PlainPasswordResult = password;
                }
            }

            StaffResult = new Staff
            {
                StaffId = _editingStaff?.StaffId ?? 0,
                FullName = fullName,
                Username = username,
                Role = role,
                Contact = contact
            };

            DialogResult = true;
            Close();
        }

        private void ShowError(string msg)
        {
            TxtError.Text = msg;
            TxtError.Visibility = Visibility.Visible;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
