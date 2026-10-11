using System;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Models;
using GymManagementSystem.Services;
using GymManagementSystem.Views.Dialogs;

namespace GymManagementSystem.Views
{
    public partial class DashboardView : UserControl
    {
        private readonly ReportService _reportService = new();
        private readonly AttendanceRepository _attRepo = new();
        private readonly MemberRepository _memberRepo = new();

        public event Action<string>? NavigationRequested;

        public DashboardView()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadDashboardData();
        }

        public void LoadDashboardData()
        {
            if (TxtTotalMembers == null || DgTodayAttendance == null) return;

            var stats = _reportService.GetDashboardStats();
            if (TxtTotalMembers != null) TxtTotalMembers.Text = stats.TotalMembers.ToString();
            if (TxtActiveMemberships != null) TxtActiveMemberships.Text = stats.ActiveMemberships.ToString();
            if (TxtExpiredMemberships != null) TxtExpiredMemberships.Text = stats.ExpiredMemberships.ToString();
            if (TxtTodayAttendance != null) TxtTodayAttendance.Text = stats.TodayAttendance.ToString();
            if (TxtTotalRevenue != null) TxtTotalRevenue.Text = $"${stats.TotalRevenue:N2}";

            var todayLogs = _attRepo.GetTodayAttendance();
            DgTodayAttendance.ItemsSource = todayLogs;
            if (TxtAttendanceCount != null)
            {
                TxtAttendanceCount.Text = $"{todayLogs.Count} members visited today";
            }
        }

        private void BtnRegisterMember_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MemberDialog
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true && dialog.MemberResult != null)
            {
                _memberRepo.Add(dialog.MemberResult);
                LoadDashboardData();
                MessageBox.Show($"Member '{dialog.MemberResult.FullName}' registered successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnQuickCheckIn_Click(object sender, RoutedEventArgs e)
        {
            NavigationRequested?.Invoke("Attendance");
        }

        private void BtnRecordPayment_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PaymentDialog
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true && dialog.RecordedPayment != null)
            {
                LoadDashboardData();
                if (dialog.ShouldOpenReceipt)
                {
                    var receiptWin = new ReceiptWindow(dialog.RecordedPayment)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    receiptWin.ShowDialog();
                }
            }
        }

        private void BtnAssignPlan_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AssignMembershipDialog(null, false)
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true)
            {
                LoadDashboardData();
                MessageBox.Show("Membership assigned successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboardData();
        }
    }
}
