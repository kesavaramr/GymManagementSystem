using System;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Services;
using GymManagementSystem.Views;

namespace GymManagementSystem
{
    public partial class MainWindow : Window
    {
        private DashboardView? _dashboardView;
        private MembersView? _membersView;
        private MembershipsView? _membershipsView;
        private TrainersView? _trainersView;
        private AttendanceView? _attendanceView;
        private PaymentsView? _paymentsView;
        private ReportsView? _reportsView;
        private StaffView? _staffView;
        private bool _isInitialized = false;

        public ContentControl ContentHost => MainContentHost;

        public MainWindow()
        {
            InitializeComponent();
            _isInitialized = true;
            SetupShell();
        }

        private void SetupShell()
        {
            var staff = AuthService.CurrentStaff;
            if (staff != null)
            {
                if (TxtStaffName != null) TxtStaffName.Text = staff.FullName;
                if (TxtStaffRole != null) TxtStaffRole.Text = $"Role: {staff.Role}";
                if (NavStaff != null) NavStaff.Visibility = AuthService.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            }

            if (TxtCurrentDateTime != null)
            {
                TxtCurrentDateTime.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
            }

            NavigateTo("Dashboard");
        }

        private void Nav_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            if (sender == NavDashboard) NavigateTo("Dashboard");
            else if (sender == NavMembers) NavigateTo("Members");
            else if (sender == NavMemberships) NavigateTo("Memberships");
            else if (sender == NavTrainers) NavigateTo("Trainers");
            else if (sender == NavAttendance) NavigateTo("Attendance");
            else if (sender == NavPayments) NavigateTo("Payments");
            else if (sender == NavReports) NavigateTo("Reports");
            else if (sender == NavStaff) NavigateTo("Staff");
        }

        public void NavigateTo(string viewName)
        {
            if (MainContentHost == null) return;

            switch (viewName)
            {
                case "Dashboard":
                    if (_dashboardView == null)
                    {
                        _dashboardView = new DashboardView();
                        _dashboardView.NavigationRequested += NavigateTo;
                    }
                    _dashboardView.LoadDashboardData();
                    MainContentHost.Content = _dashboardView;
                    if (TxtActivePageTitle != null) TxtActivePageTitle.Text = "Operations Dashboard";
                    if (NavDashboard != null && NavDashboard.IsChecked != true) NavDashboard.IsChecked = true;
                    break;

                case "Members":
                    _membersView ??= new MembersView();
                    _membersView.LoadMembers();
                    MainContentHost.Content = _membersView;
                    if (TxtActivePageTitle != null) TxtActivePageTitle.Text = "Member Management";
                    if (NavMembers != null && NavMembers.IsChecked != true) NavMembers.IsChecked = true;
                    break;

                case "Memberships":
                    _membershipsView ??= new MembershipsView();
                    _membershipsView.LoadAll();
                    MainContentHost.Content = _membershipsView;
                    if (TxtActivePageTitle != null) TxtActivePageTitle.Text = "Memberships & Plans";
                    if (NavMemberships != null && NavMemberships.IsChecked != true) NavMemberships.IsChecked = true;
                    break;

                case "Trainers":
                    _trainersView ??= new TrainersView();
                    _trainersView.LoadTrainers();
                    MainContentHost.Content = _trainersView;
                    if (TxtActivePageTitle != null) TxtActivePageTitle.Text = "Trainer Directory";
                    if (NavTrainers != null && NavTrainers.IsChecked != true) NavTrainers.IsChecked = true;
                    break;

                case "Attendance":
                    _attendanceView ??= new AttendanceView();
                    _attendanceView.LoadAttendanceLogs();
                    MainContentHost.Content = _attendanceView;
                    if (TxtActivePageTitle != null) TxtActivePageTitle.Text = "Attendance & Check-Ins";
                    if (NavAttendance != null && NavAttendance.IsChecked != true) NavAttendance.IsChecked = true;
                    break;

                case "Payments":
                    _paymentsView ??= new PaymentsView();
                    _paymentsView.LoadPayments();
                    MainContentHost.Content = _paymentsView;
                    if (TxtActivePageTitle != null) TxtActivePageTitle.Text = "Payment Processing & Receipts";
                    if (NavPayments != null && NavPayments.IsChecked != true) NavPayments.IsChecked = true;
                    break;

                case "Reports":
                    _reportsView ??= new ReportsView();
                    _reportsView.GenerateReports();
                    MainContentHost.Content = _reportsView;
                    if (TxtActivePageTitle != null) TxtActivePageTitle.Text = "Operational & Financial Reports";
                    if (NavReports != null && NavReports.IsChecked != true) NavReports.IsChecked = true;
                    break;

                case "Staff":
                    if (!AuthService.IsAdmin)
                    {
                        MessageBox.Show("Staff management is restricted to administrators.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                        NavigateTo("Dashboard");
                        return;
                    }
                    _staffView ??= new StaffView();
                    _staffView.LoadStaffAccounts();
                    MainContentHost.Content = _staffView;
                    if (TxtActivePageTitle != null) TxtActivePageTitle.Text = "Staff Accounts & Administration";
                    if (NavStaff != null && NavStaff.IsChecked != true) NavStaff.IsChecked = true;
                    break;
            }
        }

        private void BtnRefreshHeader_Click(object sender, RoutedEventArgs e)
        {
            if (NavDashboard.IsChecked == true) _dashboardView?.LoadDashboardData();
            else if (NavMembers.IsChecked == true) _membersView?.LoadMembers();
            else if (NavMemberships.IsChecked == true) _membershipsView?.LoadAll();
            else if (NavTrainers.IsChecked == true) _trainersView?.LoadTrainers();
            else if (NavAttendance.IsChecked == true) _attendanceView?.LoadAttendanceLogs();
            else if (NavPayments.IsChecked == true) _paymentsView?.LoadPayments();
            else if (NavReports.IsChecked == true) _reportsView?.GenerateReports();
            else if (NavStaff.IsChecked == true) _staffView?.LoadStaffAccounts();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure you want to log out?", "Logout Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                AuthService.Logout();
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Close();

                var loginWin = new LoginWindow();
                if (loginWin.ShowDialog() == true)
                {
                    var newMain = new MainWindow();
                    Application.Current.MainWindow = newMain;
                    Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    newMain.Show();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
        }
    }
}