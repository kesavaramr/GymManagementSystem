using System;
using System.Globalization;
using System.Windows;
using GymManagementSystem.Models;

namespace GymManagementSystem.Views.Dialogs
{
    public partial class PlanDialog : Window
    {
        public MembershipPlan? PlanResult { get; private set; }
        private readonly MembershipPlan? _editingPlan;

        public PlanDialog(MembershipPlan? existingPlan = null)
        {
            InitializeComponent();
            _editingPlan = existingPlan;

            if (_editingPlan != null)
            {
                TxtDialogTitle.Text = "Edit Membership Plan";
                BtnSave.Content = "Update Plan";

                TxtPlanName.Text = _editingPlan.PlanName;
                TxtDuration.Text = _editingPlan.DurationInMonths.ToString();
                TxtPrice.Text = _editingPlan.Price.ToString("F2");
                TxtDescription.Text = _editingPlan.Description;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var name = TxtPlanName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError("Please enter a plan name.");
                return;
            }

            if (!int.TryParse(TxtDuration.Text.Trim(), out int duration) || duration <= 0)
            {
                ShowError("Please enter a valid duration in months (e.g., 1, 3, 6, 12).");
                return;
            }

            if (!decimal.TryParse(TxtPrice.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) &&
                !decimal.TryParse(TxtPrice.Text.Trim(), out price))
            {
                ShowError("Please enter a valid price amount.");
                return;
            }

            if (price < 0)
            {
                ShowError("Price cannot be negative.");
                return;
            }

            PlanResult = new MembershipPlan
            {
                PlanId = _editingPlan?.PlanId ?? 0,
                PlanName = name,
                DurationInMonths = duration,
                Price = price,
                Description = TxtDescription.Text.Trim()
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
