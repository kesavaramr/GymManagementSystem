using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Models;
using GymManagementSystem.Services;

namespace GymManagementSystem.Views.Dialogs
{
    public partial class ReceiptWindow : Window
    {
        private readonly Payment _payment;
        private readonly string _receiptText;

        public ReceiptWindow(Payment payment)
        {
            InitializeComponent();
            _payment = payment;

            var memberRepo = new MemberRepository();
            var member = memberRepo.GetById(payment.MemberId);

            _receiptText = ReceiptService.GenerateReceiptText(_payment, member, AuthService.CurrentStaff);
            TxtReceiptContent.Text = _receiptText;
        }

        private void BtnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Text Document (*.txt)|*.txt",
                FileName = $"Receipt_{_payment.ReceiptNumber}.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                bool success = ReceiptService.SaveReceiptToFile(saveDialog.FileName, _receiptText);
                if (success)
                {
                    MessageBox.Show("Receipt successfully saved to file!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to save receipt to file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(TxtReceiptContent, $"Receipt_{_payment.ReceiptNumber}");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
