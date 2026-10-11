using System;
using System.IO;
using System.Text;
using GymManagementSystem.Models;

namespace GymManagementSystem.Services
{
    public static class ReceiptService
    {
        public static string GenerateReceiptText(Payment payment, Member? member, Staff? cashier)
        {
            var sb = new StringBuilder();
            sb.AppendLine("==================================================");
            sb.AppendLine("           SUBIRA FITNESS & GYM CENTER            ");
            sb.AppendLine("         No 15, Galle Road, Colombo 03 - Colombo        ");
            sb.AppendLine("              Phone: (+94) 75-2147-143              ");
            sb.AppendLine("==================================================");
            sb.AppendLine("                OFFICIAL RECEIPT                  ");
            sb.AppendLine("==================================================");
            sb.AppendLine($"Receipt No   : {payment.ReceiptNumber}");
            sb.AppendLine($"Payment ID   : {payment.DisplayId}");
            sb.AppendLine($"Date & Time  : {payment.PaymentDate:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Issued By    : {(cashier != null ? cashier.FullName : "System Staff")}");
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine("MEMBER DETAILS:");
            sb.AppendLine($"Member ID    : {payment.DisplayMemberId}");
            sb.AppendLine($"Member Name  : {payment.MemberName}");
            if (member != null)
            {
                sb.AppendLine($"Contact      : {member.Contact}");
            }
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine("TRANSACTION DETAILS:");
            sb.AppendLine($"Plan / Item  : {payment.PlanName}");
            sb.AppendLine($"Method       : {payment.PaymentMethod}");
            sb.AppendLine($"Status       : {payment.Status.ToUpper()}");
            if (!string.IsNullOrWhiteSpace(payment.Notes))
            {
                sb.AppendLine($"Notes        : {payment.Notes}");
            }
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine($"TOTAL PAID   : {payment.FormattedAmount,25}");
            sb.AppendLine("==================================================");
            sb.AppendLine("       Thank you for training with us!            ");
            sb.AppendLine("      No refunds after 7 days of activation.       ");
            sb.AppendLine("==================================================");
            return sb.ToString();
        }

        public static bool SaveReceiptToFile(string filePath, string receiptText)
        {
            try
            {
                File.WriteAllText(filePath, receiptText);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
