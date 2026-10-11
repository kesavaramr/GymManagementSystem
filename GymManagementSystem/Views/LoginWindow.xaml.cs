using System;
using System.Windows;
using System.Windows.Input;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Services;

namespace GymManagementSystem.Views
{
    public partial class LoginWindow : Window
    {
        private readonly StaffRepository _staffRepo = new();

        public LoginWindow()
        {
            InitializeComponent();
            TxtUsername.Focus();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            AttemptLogin();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AttemptLogin();
            }
        }

        private void AttemptLogin()
        {
            var username = TxtUsername.Text.Trim();
            var password = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter both username and password.");
                return;
            }

            var staff = _staffRepo.Authenticate(username, password);
            if (staff != null)
            {
                AuthService.SetCurrentUser(staff);
                DialogResult = true;
                Close();
            }
            else
            {
                ShowError("Invalid username or password. Please try again.");
                TxtPassword.SelectAll();
                TxtPassword.Focus();
            }
        }

        private void ShowError(string message)
        {
            TxtErrorMessage.Text = message;
            TxtErrorMessage.Visibility = Visibility.Visible;
        }
    }
}
