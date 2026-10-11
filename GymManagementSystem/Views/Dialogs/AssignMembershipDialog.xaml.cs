using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Models;

namespace GymManagementSystem.Views.Dialogs
{
    public partial class AssignMembershipDialog : Window
    {
        private readonly MemberRepository _memberRepo = new();
        private readonly MembershipRepository _membershipRepo = new();
        private readonly PaymentRepository _paymentRepo = new();

        private List<Member> _members = new();
        private List<MembershipPlan> _plans = new();

        public int CreatedMembershipId { get; private set; }
        public int CreatedPaymentId { get; private set; }

        public AssignMembershipDialog(int? preselectedMemberId = null, bool isRenewal = false)
        {
            InitializeComponent();
            LoadData(preselectedMemberId, isRenewal);
        }

        private void LoadData(int? preselectedMemberId, bool isRenewal)
        {
            _members = _memberRepo.GetAll();
            _plans = _membershipRepo.GetAllPlans();

            CmbMember.ItemsSource = _members;
            CmbPlan.ItemsSource = _plans;

            DpStartDate.SelectedDate = DateTime.Today;

            if (preselectedMemberId.HasValue)
            {
                var target = _members.FirstOrDefault(m => m.MemberId == preselectedMemberId.Value);
                if (target != null)
                {
                    CmbMember.SelectedItem = target;
                    CmbMember.IsEnabled = false; // Lock to selected member
                }
            }
            else if (_members.Count > 0)
            {
                CmbMember.SelectedIndex = 0;
            }

            if (_plans.Count > 0)
            {
                CmbPlan.SelectedIndex = 0;
            }

            if (isRenewal)
            {
                TxtTitle.Text = "Renew Membership";
                BtnSubmit.Content = "Renew Plan";
            }

            UpdateCalculations();
        }

        private void CmbMember_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbMember.SelectedItem is Member member)
            {
                var latest = _membershipRepo.GetLatestMembershipByMemberId(member.MemberId);
                if (latest != null && latest.ExpiryDate > DateTime.Today)
                {
                    DpStartDate.SelectedDate = latest.ExpiryDate.AddDays(1);
                }
                else
                {
                    DpStartDate.SelectedDate = DateTime.Today;
                }
            }
            UpdateCalculations();
        }

        private void CmbPlan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCalculations();
        }

        private void DpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCalculations();
        }

        private void UpdateCalculations()
        {
            if (CmbPlan?.SelectedItem is MembershipPlan plan)
            {
                TxtPlanPrice.Text = plan.FormattedPrice;
                TxtPlanDuration.Text = plan.FormattedDuration;

                var start = DpStartDate?.SelectedDate ?? DateTime.Today;
                var expiry = start.AddMonths(plan.DurationInMonths);
                TxtExpiryDate.Text = expiry.ToString("yyyy-MM-dd");
            }
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (CmbMember.SelectedItem is not Member member)
            {
                ShowError("Please select a member.");
                return;
            }

            if (CmbPlan.SelectedItem is not MembershipPlan plan)
            {
                ShowError("Please select a membership plan.");
                return;
            }

            var startDate = DpStartDate.SelectedDate ?? DateTime.Today;
            var notes = TxtNotes.Text.Trim();

            try
            {
                CreatedMembershipId = _membershipRepo.AssignMembership(member.MemberId, plan.PlanId, startDate, notes);

                if (ChkRecordPayment.IsChecked == true)
                {
                    var payment = new Payment
                    {
                        MemberId = member.MemberId,
                        MemberName = member.FullName,
                        MembershipId = CreatedMembershipId,
                        PlanName = plan.PlanName,
                        Amount = plan.Price,
                        PaymentDate = DateTime.Today,
                        PaymentMethod = "Cash",
                        Status = "Paid",
                        Notes = $"Membership fee for {plan.PlanName}"
                    };
                    CreatedPaymentId = _paymentRepo.RecordPayment(payment);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Error assigning membership: {ex.Message}");
            }
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
