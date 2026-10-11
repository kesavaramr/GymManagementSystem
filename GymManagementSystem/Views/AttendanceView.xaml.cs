using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Models;

namespace GymManagementSystem.Views
{
    public partial class AttendanceView : UserControl
    {
        private readonly AttendanceRepository _attRepo = new();
        private readonly MemberRepository _memberRepo = new();

        private List<Member> _members = new();

        public AttendanceView()
        {
            InitializeComponent();
            Loaded += (s, e) => InitializeView();
        }

        private void InitializeView()
        {
            LoadMembersDropdown();
            DpFilterDate.SelectedDate = DateTime.Today;
            LoadAttendanceLogs();
        }

        private void LoadMembersDropdown()
        {
            _members = _memberRepo.GetAll();
            CmbCheckInMember.ItemsSource = _members;
            if (_members.Count > 0)
            {
                CmbCheckInMember.SelectedIndex = 0;
            }
        }

        private void CmbCheckInMember_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TxtMemberStatus == null) return;

            if (CmbCheckInMember.SelectedItem is Member member)
            {
                TxtMemberStatus.Text = $"{member.MembershipStatus} ({member.CurrentPlanName})";
                if (member.MembershipStatus.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    TxtMemberStatus.Foreground = System.Windows.Media.Brushes.DarkGreen;
                }
                else
                {
                    TxtMemberStatus.Foreground = System.Windows.Media.Brushes.Crimson;
                }
            }
        }

        public void LoadAttendanceLogs()
        {
            if (DgAttendance == null) return;

            var memberQuery = TxtFilterMember?.Text?.Trim();
            var date = DpFilterDate?.SelectedDate;

            var logs = _attRepo.SearchAttendance(memberQuery, date);
            DgAttendance.ItemsSource = logs;

            var dateDesc = date.HasValue ? $"for {date.Value:yyyy-MM-dd}" : "for all dates";
            if (TxtFooterCount != null)
            {
                TxtFooterCount.Text = $"Showing {logs.Count} attendance check-in(s) {dateDesc}";
            }
        }

        private void Filter_Changed(object sender, TextChangedEventArgs e)
        {
            LoadAttendanceLogs();
        }

        private void DpFilterDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAttendanceLogs();
        }

        private void BtnToday_Click(object sender, RoutedEventArgs e)
        {
            DpFilterDate.SelectedDate = DateTime.Today;
        }

        private void BtnAllDates_Click(object sender, RoutedEventArgs e)
        {
            DpFilterDate.SelectedDate = null;
            LoadAttendanceLogs();
        }

        private void BtnRecordAttendance_Click(object sender, RoutedEventArgs e)
        {
            if (CmbCheckInMember.SelectedItem is not Member member)
            {
                MessageBox.Show("Please select a member to check in.", "Missing Member", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_attRepo.IsAlreadyCheckedInToday(member.MemberId))
            {
                var proceed = MessageBox.Show(
                    $"Member '{member.FullName}' has already checked in today.\nDo you want to record another check-in anyway?",
                    "Already Checked In Today",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (proceed != MessageBoxResult.Yes) return;
            }

            if (!member.MembershipStatus.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                var warn = MessageBox.Show(
                    $"Notice: Member '{member.FullName}' has membership status: {member.MembershipStatus.ToUpper()}.\nDo you want to proceed with check-in?",
                    "Membership Status Notice",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (warn != MessageBoxResult.Yes) return;
            }

            var notes = TxtCheckInNotes.Text.Trim();
            _attRepo.RecordAttendance(member.MemberId, notes);

            TxtCheckInNotes.Text = string.Empty;
            LoadAttendanceLogs();

            MessageBox.Show($"Check-in recorded for {member.FullName} at {DateTime.Now:hh:mm tt}!", "Check-In Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDeleteLog_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Attendance att)
            {
                var res = MessageBox.Show($"Remove check-in log #{att.AttendanceId} for {att.MemberName}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    _attRepo.Delete(att.AttendanceId);
                    LoadAttendanceLogs();
                }
            }
        }
    }
}
