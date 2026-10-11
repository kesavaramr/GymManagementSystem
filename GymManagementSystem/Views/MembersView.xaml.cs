using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Models;
using GymManagementSystem.Views.Dialogs;

namespace GymManagementSystem.Views
{
    public partial class MembersView : UserControl
    {
        private readonly MemberRepository _memberRepo = new();

        public MembersView()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadMembers();
        }

        public void LoadMembers()
        {
            if (DgMembers == null) return;

            var search = TxtSearch?.Text?.Trim();
            var status = (CmbStatusFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";

            var list = _memberRepo.GetAll(search, status);
            DgMembers.ItemsSource = list;
            if (TxtFooterCount != null)
            {
                TxtFooterCount.Text = $"Showing {list.Count} registered member(s)";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadMembers();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadMembers();
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Text = string.Empty;
            CmbStatusFilter.SelectedIndex = 0;
            LoadMembers();
        }

        private void BtnAddMember_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MemberDialog
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true && dialog.MemberResult != null)
            {
                int newId = _memberRepo.Add(dialog.MemberResult);
                LoadMembers();
                MessageBox.Show($"Member '{dialog.MemberResult.FullName}' registered successfully! (ID: MEM-{newId:D4})", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnEditMember_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Member member)
            {
                var dialog = new MemberDialog(member)
                {
                    Owner = Window.GetWindow(this)
                };
                if (dialog.ShowDialog() == true && dialog.MemberResult != null)
                {
                    _memberRepo.Update(dialog.MemberResult);
                    LoadMembers();
                    MessageBox.Show($"Member '{dialog.MemberResult.FullName}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnAssignPlan_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Member member)
            {
                var dialog = new AssignMembershipDialog(member.MemberId, member.MembershipStatus.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    Owner = Window.GetWindow(this)
                };
                if (dialog.ShowDialog() == true)
                {
                    LoadMembers();
                    MessageBox.Show($"Membership assigned/renewed for {member.FullName}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnDeleteMember_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Member member)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete member '{member.FullName}' (ID: {member.DisplayId})?\nAll associated attendance and payment records will also be removed.",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _memberRepo.Delete(member.MemberId);
                    LoadMembers();
                    MessageBox.Show("Member deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
