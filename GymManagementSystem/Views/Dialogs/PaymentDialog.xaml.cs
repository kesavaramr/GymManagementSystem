using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Models;

namespace GymManagementSystem.Views.Dialogs
{
    public partial class PaymentDialog : Window
    {
        private readonly MemberRepository _memberRepo = new();
        private readonly MembershipRepository _membershipRepo = new();
        private readonly PaymentRepository _paymentRepo = new();

        private List<Member> _members = new();
        private List<MembershipPlan> _plans = new();

        public Payment? RecordedPayment { get; private set; }
        public bool ShouldOpenReceipt => ChkOpenReceipt.IsChecked == true;

        public PaymentDialog(int? preselectedMemberId = null)
        {
            InitializeComponent();
            LoadData(preselectedMemberId);
        }

        private void LoadData(int? preselectedMemberId)
        {
            _members = _memberRepo.GetAll();
            _plans = _membershipRepo.GetAllPlans();

            CmbMember.ItemsSource = _members;
            CmbPlan.ItemsSource = _plans;

            DpPaymentDate.SelectedDate = DateTime.Today;

            if (preselectedMemberId.HasValue)
            {
                var member = _members.FirstOrDefault(m => m.MemberId == preselectedMemberId.Value);
                if (member != null)
                {
                    CmbMember.SelectedItem = member;
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
        }

        private void CmbMember_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbMember.SelectedItem is Member member)
            {
                // Auto-select latest plan if available
                var latest = _membershipRepo.GetLatestMembershipByMemberId(member.MemberId);
                if (latest != null)
                {
                    var matchPlan = _plans.FirstOrDefault(p => p.PlanId == latest.PlanId);
                    if (matchPlan != null)
                    {
                        CmbPlan.SelectedItem = matchPlan;
                    }
                }
            }
        }

        private void CmbPlan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbPlan.SelectedItem is MembershipPlan plan)
            {
                TxtAmount.Text = plan.Price.ToString("F2");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CmbMember.SelectedItem is not Member member)
            {
                ShowError("Please select a member.");
                return;
            }

            if (!decimal.TryParse(TxtAmount.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) &&
                !decimal.TryParse(TxtAmount.Text.Trim(), out amount))
            {
                ShowError("Please enter a valid payment amount.");
                return;
            }

            if (amount <= 0)
            {
                ShowError("Amount must be greater than zero.");
                return;
            }

            var plan = CmbPlan.SelectedItem as MembershipPlan;
            var method = (CmbMethod.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Cash";
            var status = (CmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Paid";
            var payDate = DpPaymentDate.SelectedDate ?? DateTime.Today;
            var notes = TxtNotes.Text.Trim();

            // Link to latest membership if exists
            var latestMembership = _membershipRepo.GetLatestMembershipByMemberId(member.MemberId);

            var payment = new Payment
            {
                MemberId = member.MemberId,
                MemberName = member.FullName,
                MembershipId = latestMembership?.MembershipId,
                PlanName = plan != null ? plan.PlanName : (latestMembership != null ? latestMembership.PlanName : "Direct Fee"),
                Amount = amount,
                PaymentDate = payDate,
                PaymentMethod = method,
                Status = status,
                Notes = notes
            };

            try
            {
                int payId = _paymentRepo.RecordPayment(payment);
                RecordedPayment = _paymentRepo.GetPaymentById(payId);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to save payment: {ex.Message}");
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
