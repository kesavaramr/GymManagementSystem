using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymManagementSystem.Data;
using GymManagementSystem.Models;

namespace GymManagementSystem.Data.Repositories
{
    public class MembershipRepository
    {
        // ==================== PLANS ====================

        public List<MembershipPlan> GetAllPlans()
        {
            var list = new List<MembershipPlan>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT plan_id, plan_name, duration_months, price, description FROM membership_plans ORDER BY duration_months ASC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new MembershipPlan
                {
                    PlanId = reader.GetInt32(0),
                    PlanName = reader.GetString(1),
                    DurationInMonths = reader.GetInt32(2),
                    Price = reader.GetDecimal(3),
                    Description = reader.IsDBNull(4) ? "" : reader.GetString(4)
                });
            }
            return list;
        }

        public MembershipPlan? GetPlanById(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT plan_id, plan_name, duration_months, price, description FROM membership_plans WHERE plan_id = @id;";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new MembershipPlan
                {
                    PlanId = reader.GetInt32(0),
                    PlanName = reader.GetString(1),
                    DurationInMonths = reader.GetInt32(2),
                    Price = reader.GetDecimal(3),
                    Description = reader.IsDBNull(4) ? "" : reader.GetString(4)
                };
            }
            return null;
        }

        public int AddPlan(MembershipPlan plan)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO membership_plans (plan_name, duration_months, price, description)
                VALUES (@name, @dur, @price, @desc);
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("@name", plan.PlanName);
            cmd.Parameters.AddWithValue("@dur", plan.DurationInMonths);
            cmd.Parameters.AddWithValue("@price", plan.Price);
            cmd.Parameters.AddWithValue("@desc", plan.Description ?? "");

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool UpdatePlan(MembershipPlan plan)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE membership_plans 
                SET plan_name = @name, duration_months = @dur, price = @price, description = @desc
                WHERE plan_id = @id;
            ";
            cmd.Parameters.AddWithValue("@id", plan.PlanId);
            cmd.Parameters.AddWithValue("@name", plan.PlanName);
            cmd.Parameters.AddWithValue("@dur", plan.DurationInMonths);
            cmd.Parameters.AddWithValue("@price", plan.Price);
            cmd.Parameters.AddWithValue("@desc", plan.Description ?? "");

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeletePlan(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM membership_plans WHERE plan_id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        // ==================== MEMBER MEMBERSHIPS ====================

        public List<MemberMembership> GetAllMemberships()
        {
            var list = new List<MemberMembership>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT m.membership_id, m.member_id, mem.name, m.plan_id, p.plan_name, m.start_date, m.expiry_date, m.status, m.notes
                FROM memberships m
                INNER JOIN members mem ON m.member_id = mem.member_id
                INNER JOIN membership_plans p ON m.plan_id = p.plan_id
                ORDER BY m.membership_id DESC;
            ";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var expiry = DateTime.Parse(reader.GetString(6));
                var storedStatus = reader.GetString(7);
                var effectiveStatus = (expiry < DateTime.Today && storedStatus == "Active") ? "Expired" : storedStatus;

                list.Add(new MemberMembership
                {
                    MembershipId = reader.GetInt32(0),
                    MemberId = reader.GetInt32(1),
                    MemberName = reader.GetString(2),
                    PlanId = reader.GetInt32(3),
                    PlanName = reader.GetString(4),
                    StartDate = DateTime.Parse(reader.GetString(5)),
                    ExpiryDate = expiry,
                    Status = effectiveStatus,
                    Notes = reader.IsDBNull(8) ? "" : reader.GetString(8)
                });
            }
            return list;
        }

        public MemberMembership? GetLatestMembershipByMemberId(int memberId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT m.membership_id, m.member_id, mem.name, m.plan_id, p.plan_name, m.start_date, m.expiry_date, m.status, m.notes
                FROM memberships m
                INNER JOIN members mem ON m.member_id = mem.member_id
                INNER JOIN membership_plans p ON m.plan_id = p.plan_id
                WHERE m.member_id = @memberId
                ORDER BY m.expiry_date DESC
                LIMIT 1;
            ";
            cmd.Parameters.AddWithValue("@memberId", memberId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var expiry = DateTime.Parse(reader.GetString(6));
                var storedStatus = reader.GetString(7);
                var effectiveStatus = (expiry < DateTime.Today && storedStatus == "Active") ? "Expired" : storedStatus;

                return new MemberMembership
                {
                    MembershipId = reader.GetInt32(0),
                    MemberId = reader.GetInt32(1),
                    MemberName = reader.GetString(2),
                    PlanId = reader.GetInt32(3),
                    PlanName = reader.GetString(4),
                    StartDate = DateTime.Parse(reader.GetString(5)),
                    ExpiryDate = expiry,
                    Status = effectiveStatus,
                    Notes = reader.IsDBNull(8) ? "" : reader.GetString(8)
                };
            }
            return null;
        }

        public int AssignMembership(int memberId, int planId, DateTime startDate, string notes = "")
        {
            var plan = GetPlanById(planId);
            if (plan == null) throw new ArgumentException("Plan not found");

            var expiryDate = startDate.AddMonths(plan.DurationInMonths);

            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO memberships (member_id, plan_id, start_date, expiry_date, status, notes)
                VALUES (@memberId, @planId, @startDate, @expiryDate, 'Active', @notes);
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("@memberId", memberId);
            cmd.Parameters.AddWithValue("@planId", planId);
            cmd.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@expiryDate", expiryDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@notes", notes ?? "");

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool RenewMembership(int memberId, int planId, string notes = "")
        {
            var plan = GetPlanById(planId);
            if (plan == null) throw new ArgumentException("Plan not found");

            var latest = GetLatestMembershipByMemberId(memberId);
            DateTime startDate = DateTime.Today;
            if (latest != null && latest.ExpiryDate > DateTime.Today)
            {
                // Extend from existing expiry date
                startDate = latest.ExpiryDate.AddDays(1);
            }
            var expiryDate = startDate.AddMonths(plan.DurationInMonths);

            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO memberships (member_id, plan_id, start_date, expiry_date, status, notes)
                VALUES (@memberId, @planId, @startDate, @expiryDate, 'Active', @notes);
            ";
            cmd.Parameters.AddWithValue("@memberId", memberId);
            cmd.Parameters.AddWithValue("@planId", planId);
            cmd.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@expiryDate", expiryDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@notes", string.IsNullOrEmpty(notes) ? "Membership renewal" : notes);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateStatus(int membershipId, string status)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE memberships SET status = @status WHERE membership_id = @id;";
            cmd.Parameters.AddWithValue("@id", membershipId);
            cmd.Parameters.AddWithValue("@status", status);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
