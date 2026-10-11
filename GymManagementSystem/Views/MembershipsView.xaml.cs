using System;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Models;
using GymManagementSystem.Views.Dialogs;

namespace GymManagementSystem.Views
{
    public partial class MembershipsView : UserControl
    {
        private readonly MembershipRepository _membershipRepo = new();

        public MembershipsView()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadAll();
        }

        public void LoadAll()
        {
            LoadSubscriptions();
            LoadPlans();
        }

        private void LoadSubscriptions()
        {
            if (DgMemberships == null) return;
            var list = _membershipRepo.GetAllMemberships();
            DgMemberships.ItemsSource = list;
            if (TxtSubscriptionCount != null)
            {
                TxtSubscriptionCount.Text = $"Showing {list.Count} subscription record(s)";
            }
        }

        private void LoadPlans()
        {
            if (DgPlans == null) return;
            var plans = _membershipRepo.GetAllPlans();
            DgPlans.ItemsSource = plans;
            if (TxtPlanCount != null)
            {
                TxtPlanCount.Text = $"Showing {plans.Count} available plan(s)";
            }
        }

        private void BtnRefreshSubscriptions_Click(object sender, RoutedEventArgs e)
        {
            LoadSubscriptions();
        }

        private void BtnAssignMembership_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AssignMembershipDialog(null, false)
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true)
            {
                LoadSubscriptions();
                MessageBox.Show("Membership successfully assigned!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnRenew_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is MemberMembership mem)
            {
                var dialog = new AssignMembershipDialog(mem.MemberId, true)
                {
                    Owner = Window.GetWindow(this)
                };
                if (dialog.ShowDialog() == true)
                {
                    LoadSubscriptions();
                    MessageBox.Show($"Membership renewed for {mem.MemberName}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnAddPlan_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PlanDialog
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true && dialog.PlanResult != null)
            {
                _membershipRepo.AddPlan(dialog.PlanResult);
                LoadPlans();
                MessageBox.Show($"Plan '{dialog.PlanResult.PlanName}' created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnEditPlan_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is MembershipPlan plan)
            {
                var dialog = new PlanDialog(plan)
                {
                    Owner = Window.GetWindow(this)
                };
                if (dialog.ShowDialog() == true && dialog.PlanResult != null)
                {
                    _membershipRepo.UpdatePlan(dialog.PlanResult);
                    LoadPlans();
                    MessageBox.Show($"Plan '{dialog.PlanResult.PlanName}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnDeletePlan_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is MembershipPlan plan)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the plan '{plan.PlanName}'?\nNote: If this plan is assigned to members, deletion may be restricted.",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool deleted = _membershipRepo.DeletePlan(plan.PlanId);
                        if (deleted)
                        {
                            LoadPlans();
                            MessageBox.Show("Plan deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Cannot delete plan because it is currently assigned to members. Error: {ex.Message}", "Action Blocked", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }
    }
}
