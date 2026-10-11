using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymManagementSystem.Data;
using GymManagementSystem.Models;

namespace GymManagementSystem.Data.Repositories
{
    public class PaymentRepository
    {
        public string GenerateNextReceiptNumber()
        {
            var year = DateTime.Today.Year;
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM payments;";
            var count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
            return $"REC-{year}-{count:D4}";
        }

        public int RecordPayment(Payment payment)
        {
            if (string.IsNullOrEmpty(payment.ReceiptNumber))
            {
                payment.ReceiptNumber = GenerateNextReceiptNumber();
            }

            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO payments (member_id, membership_id, amount, payment_date, payment_method, status, receipt_number, notes)
                VALUES (@memberId, @membershipId, @amount, @date, @method, @status, @receipt, @notes);
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("@memberId", payment.MemberId);
            cmd.Parameters.AddWithValue("@membershipId", payment.MembershipId.HasValue ? payment.MembershipId.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@amount", payment.Amount);
            cmd.Parameters.AddWithValue("@date", payment.PaymentDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@method", payment.PaymentMethod);
            cmd.Parameters.AddWithValue("@status", payment.Status);
            cmd.Parameters.AddWithValue("@receipt", payment.ReceiptNumber);
            cmd.Parameters.AddWithValue("@notes", payment.Notes ?? "");

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<Payment> GetAllPayments(string? statusFilter = null, string? memberQuery = null)
        {
            var list = new List<Payment>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT 
                    p.payment_id, 
                    p.member_id, 
                    m.name, 
                    p.membership_id, 
                    IFNULL(plan.plan_name, 'Direct Payment') AS plan_name,
                    p.amount, 
                    p.payment_date, 
                    p.payment_method, 
                    p.status, 
                    p.receipt_number, 
                    p.notes
                FROM payments p
                INNER JOIN members m ON p.member_id = m.member_id
                LEFT JOIN memberships ms ON p.membership_id = ms.membership_id
                LEFT JOIN membership_plans plan ON ms.plan_id = plan.plan_id
                WHERE 1=1
            ";

            if (!string.IsNullOrWhiteSpace(statusFilter) && !statusFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                cmd.CommandText += " AND p.status = @status";
                cmd.Parameters.AddWithValue("@status", statusFilter);
            }

            if (!string.IsNullOrWhiteSpace(memberQuery))
            {
                cmd.CommandText += " AND (m.name LIKE @q OR p.receipt_number LIKE @q OR printf('MEM-%04d', p.member_id) LIKE @q OR CAST(p.member_id AS TEXT) LIKE @q)";
                cmd.Parameters.AddWithValue("@q", $"%{memberQuery.Trim()}%");
            }

            cmd.CommandText += " ORDER BY p.payment_id DESC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Payment
                {
                    PaymentId = reader.GetInt32(0),
                    MemberId = reader.GetInt32(1),
                    MemberName = reader.GetString(2),
                    MembershipId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    PlanName = reader.GetString(4),
                    Amount = reader.GetDecimal(5),
                    PaymentDate = DateTime.Parse(reader.GetString(6)),
                    PaymentMethod = reader.GetString(7),
                    Status = reader.GetString(8),
                    ReceiptNumber = reader.GetString(9),
                    Notes = reader.IsDBNull(10) ? "" : reader.GetString(10)
                });
            }
            return list;
        }

        public List<Payment> GetPaymentsByMember(int memberId)
        {
            var list = new List<Payment>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    p.payment_id, 
                    p.member_id, 
                    m.name, 
                    p.membership_id, 
                    IFNULL(plan.plan_name, 'Direct Payment') AS plan_name,
                    p.amount, 
                    p.payment_date, 
                    p.payment_method, 
                    p.status, 
                    p.receipt_number, 
                    p.notes
                FROM payments p
                INNER JOIN members m ON p.member_id = m.member_id
                LEFT JOIN memberships ms ON p.membership_id = ms.membership_id
                LEFT JOIN membership_plans plan ON ms.plan_id = plan.plan_id
                WHERE p.member_id = @id
                ORDER BY p.payment_id DESC;
            ";
            cmd.Parameters.AddWithValue("@id", memberId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Payment
                {
                    PaymentId = reader.GetInt32(0),
                    MemberId = reader.GetInt32(1),
                    MemberName = reader.GetString(2),
                    MembershipId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    PlanName = reader.GetString(4),
                    Amount = reader.GetDecimal(5),
                    PaymentDate = DateTime.Parse(reader.GetString(6)),
                    PaymentMethod = reader.GetString(7),
                    Status = reader.GetString(8),
                    ReceiptNumber = reader.GetString(9),
                    Notes = reader.IsDBNull(10) ? "" : reader.GetString(10)
                });
            }
            return list;
        }

        public Payment? GetPaymentById(int paymentId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    p.payment_id, 
                    p.member_id, 
                    m.name, 
                    p.membership_id, 
                    IFNULL(plan.plan_name, 'Direct Payment') AS plan_name,
                    p.amount, 
                    p.payment_date, 
                    p.payment_method, 
                    p.status, 
                    p.receipt_number, 
                    p.notes
                FROM payments p
                INNER JOIN members m ON p.member_id = m.member_id
                LEFT JOIN memberships ms ON p.membership_id = ms.membership_id
                LEFT JOIN membership_plans plan ON ms.plan_id = plan.plan_id
                WHERE p.payment_id = @id;
            ";
            cmd.Parameters.AddWithValue("@id", paymentId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Payment
                {
                    PaymentId = reader.GetInt32(0),
                    MemberId = reader.GetInt32(1),
                    MemberName = reader.GetString(2),
                    MembershipId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    PlanName = reader.GetString(4),
                    Amount = reader.GetDecimal(5),
                    PaymentDate = DateTime.Parse(reader.GetString(6)),
                    PaymentMethod = reader.GetString(7),
                    Status = reader.GetString(8),
                    ReceiptNumber = reader.GetString(9),
                    Notes = reader.IsDBNull(10) ? "" : reader.GetString(10)
                };
            }
            return null;
        }

        public bool UpdatePaymentStatus(int paymentId, string status)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE payments SET status = @status WHERE payment_id = @id;";
            cmd.Parameters.AddWithValue("@id", paymentId);
            cmd.Parameters.AddWithValue("@status", status);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeletePayment(int paymentId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM payments WHERE payment_id = @id;";
            cmd.Parameters.AddWithValue("@id", paymentId);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
