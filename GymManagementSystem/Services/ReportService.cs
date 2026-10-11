using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymManagementSystem.Data;
using GymManagementSystem.Models;

namespace GymManagementSystem.Services
{
    public class ReportService
    {
        public DashboardStats GetDashboardStats()
        {
            var stats = new DashboardStats();
            var today = DateTime.Today.ToString("yyyy-MM-dd");

            using var conn = DatabaseHelper.GetConnection();

            // Total Members
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM members;";
                stats.TotalMembers = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Active Memberships
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM memberships WHERE expiry_date >= @today AND status = 'Active';";
                cmd.Parameters.AddWithValue("@today", today);
                stats.ActiveMemberships = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Expired Memberships
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM memberships WHERE expiry_date < @today OR status = 'Expired';";
                cmd.Parameters.AddWithValue("@today", today);
                stats.ExpiredMemberships = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Today's Attendance
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM attendance WHERE attendance_date = @today;";
                cmd.Parameters.AddWithValue("@today", today);
                stats.TodayAttendance = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Total Revenue
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT IFNULL(SUM(amount), 0) FROM payments WHERE status = 'Paid';";
                stats.TotalRevenue = Convert.ToDecimal(cmd.ExecuteScalar());
            }

            return stats;
        }

        public class MembershipReportItem
        {
            public string Category { get; set; } = string.Empty;
            public int Count { get; set; }
            public double Percentage { get; set; }
        }

        public List<MembershipReportItem> GetMembershipStatusReport()
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var result = new List<MembershipReportItem>();

            using var conn = DatabaseHelper.GetConnection();

            using var totalCmd = conn.CreateCommand();
            totalCmd.CommandText = "SELECT COUNT(*) FROM members;";
            int total = Convert.ToInt32(totalCmd.ExecuteScalar());
            if (total == 0) return result;

            // Active count
            using var activeCmd = conn.CreateCommand();
            activeCmd.CommandText = @"
                SELECT COUNT(DISTINCT member_id) 
                FROM memberships 
                WHERE expiry_date >= @today AND status = 'Active';
            ";
            activeCmd.Parameters.AddWithValue("@today", today);
            int active = Convert.ToInt32(activeCmd.ExecuteScalar());

            // Expired count (members who have a membership, but latest is expired)
            using var expCmd = conn.CreateCommand();
            expCmd.CommandText = @"
                SELECT COUNT(DISTINCT member_id) 
                FROM memberships 
                WHERE member_id NOT IN (
                    SELECT member_id FROM memberships WHERE expiry_date >= @today AND status = 'Active'
                );
            ";
            expCmd.Parameters.AddWithValue("@today", today);
            int expired = Convert.ToInt32(expCmd.ExecuteScalar());

            int inactive = total - (active + expired);
            if (inactive < 0) inactive = 0;

            result.Add(new MembershipReportItem { Category = "Active Memberships", Count = active, Percentage = Math.Round((double)active / total * 100, 1) });
            result.Add(new MembershipReportItem { Category = "Expired Memberships", Count = expired, Percentage = Math.Round((double)expired / total * 100, 1) });
            result.Add(new MembershipReportItem { Category = "No Active Plan / Inactive", Count = inactive, Percentage = Math.Round((double)inactive / total * 100, 1) });

            return result;
        }

        public class FinancialReportSummary
        {
            public decimal TotalPaid { get; set; }
            public decimal TotalPending { get; set; }
            public decimal TotalOverdue { get; set; }
            public int TransactionCount { get; set; }
            public Dictionary<string, decimal> ByPaymentMethod { get; set; } = new();
        }

        public FinancialReportSummary GetFinancialReport(DateTime startDate, DateTime endDate)
        {
            var summary = new FinancialReportSummary();
            using var conn = DatabaseHelper.GetConnection();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT status, payment_method, amount 
                FROM payments 
                WHERE payment_date >= @s AND payment_date <= @e;
            ";
            cmd.Parameters.AddWithValue("@s", startDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@e", endDate.ToString("yyyy-MM-dd"));

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var status = reader.GetString(0);
                var method = reader.GetString(1);
                var amount = reader.GetDecimal(2);

                summary.TransactionCount++;

                if (status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                {
                    summary.TotalPaid += amount;
                    if (!summary.ByPaymentMethod.ContainsKey(method))
                    {
                        summary.ByPaymentMethod[method] = 0;
                    }
                    summary.ByPaymentMethod[method] += amount;
                }
                else if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                {
                    summary.TotalPending += amount;
                }
                else if (status.Equals("Overdue", StringComparison.OrdinalIgnoreCase))
                {
                    summary.TotalOverdue += amount;
                }
            }

            return summary;
        }
    }
}
