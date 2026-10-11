using System;
using System.Windows;
using GymManagementSystem.Data;
using GymManagementSystem.Views;

namespace GymManagementSystem
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Catch any unexpected dispatcher exceptions
            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show(
                    $"An unexpected error occurred:\n{args.Exception.Message}\n\n{args.Exception.StackTrace}",
                    "Application Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                args.Handled = true;
            };

            // Prevent premature shutdown when LoginWindow closes
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            try
            {
                // Initialize SQLite tables and seed data
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize database: {ex.Message}", "Fatal Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var loginWindow = new LoginWindow();
            bool? loginResult = loginWindow.ShowDialog();

            if (loginResult == true)
            {
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                // Once main window is created, shut down when main window closes
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }
}
