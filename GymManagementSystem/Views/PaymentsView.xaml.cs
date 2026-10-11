using System;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Models;
using GymManagementSystem.Views.Dialogs;

namespace GymManagementSystem.Views
{
    public partial class PaymentsView : UserControl
    {
        private readonly PaymentRepository _paymentRepo = new();

        public PaymentsView()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadPayments();
        }

        public void LoadPayments()
        {
            if (DgPayments == null) return;

            var search = TxtSearch?.Text?.Trim();
            var status = (CmbStatusFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";

            var list = _paymentRepo.GetAllPayments(status, search);
            DgPayments.ItemsSource = list;
            if (TxtFooterCount != null)
            {
                TxtFooterCount.Text = $"Showing {list.Count} payment transaction(s)";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadPayments();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadPayments();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Text = string.Empty;
            CmbStatusFilter.SelectedIndex = 0;
            LoadPayments();
        }

        private void BtnRecordPayment_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PaymentDialog
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true && dialog.RecordedPayment != null)
            {
                LoadPayments();
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

        private void BtnViewReceipt_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Payment payment)
            {
                var receiptWin = new ReceiptWindow(payment)
                {
                    Owner = Window.GetWindow(this)
                };
                receiptWin.ShowDialog();
            }
        }

        private void BtnMarkPaid_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Payment payment)
            {
                if (payment.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("This payment is already marked as Paid.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _paymentRepo.UpdatePaymentStatus(payment.PaymentId, "Paid");
                LoadPayments();
                MessageBox.Show($"Payment {payment.DisplayId} status updated to Paid.", "Updated", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeletePayment_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Payment payment)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete payment #{payment.DisplayId} ({payment.ReceiptNumber}) for {payment.MemberName}?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _paymentRepo.DeletePayment(payment.PaymentId);
                    LoadPayments();
                    MessageBox.Show("Payment record deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
