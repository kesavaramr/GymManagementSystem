using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymManagementSystem.Data;
using GymManagementSystem.Models;

namespace GymManagementSystem.Data.Repositories
{
    public class MemberRepository
    {
        public List<Member> GetAll(string? searchQuery = null, string? statusFilter = null)
        {
            var list = new List<Member>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();

            // Query member details with assigned trainer and latest membership info
            cmd.CommandText = @"
                SELECT 
                    m.member_id, 
                    m.name, 
                    m.date_of_birth, 
                    m.gender, 
                    m.contact, 
                    m.address, 
                    m.registration_date, 
                    m.assigned_trainer_id,
                    t.name AS trainer_name,
                    p.plan_name,
                    mem_ship.expiry_date,
                    mem_ship.status
                FROM members m
                LEFT JOIN trainers t ON m.assigned_trainer_id = t.trainer_id
                LEFT JOIN (
                    SELECT m1.member_id, m1.plan_id, m1.expiry_date, m1.status
                    FROM memberships m1
                    INNER JOIN (
                        SELECT member_id, MAX(expiry_date) as max_exp
                        FROM memberships
                        GROUP BY member_id
                    ) m2 ON m1.member_id = m2.member_id AND m1.expiry_date = m2.max_exp
                ) mem_ship ON m.member_id = mem_ship.member_id
                LEFT JOIN membership_plans p ON mem_ship.plan_id = p.plan_id
                WHERE 1=1
            ";

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                cmd.CommandText += " AND (m.name LIKE @q OR m.contact LIKE @q OR printf('MEM-%04d', m.member_id) LIKE @q OR CAST(m.member_id AS TEXT) LIKE @q)";
                cmd.Parameters.AddWithValue("@q", $"%{searchQuery.Trim()}%");
            }

            cmd.CommandText += " ORDER BY m.member_id DESC;";

            using var reader = cmd.ExecuteReader();
            var today = DateTime.Today;

            while (reader.Read())
            {
                var member = new Member
                {
                    MemberId = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    DateOfBirth = reader.IsDBNull(2) ? null : DateTime.Parse(reader.GetString(2)),
                    Gender = reader.GetString(3),
                    Contact = reader.GetString(4),
                    Address = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    RegistrationDate = DateTime.Parse(reader.GetString(6)),
                    AssignedTrainerId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    AssignedTrainerName = reader.IsDBNull(8) ? "None" : reader.GetString(8),
                    CurrentPlanName = reader.IsDBNull(9) ? "None" : reader.GetString(9)
                };

                if (!reader.IsDBNull(10))
                {
                    var expDate = DateTime.Parse(reader.GetString(10));
                    member.CurrentExpiryDate = expDate;
                    var storedStatus = reader.GetString(11);

                    if (storedStatus.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        member.MembershipStatus = expDate >= today ? "Active" : "Expired";
                    }
                    else
                    {
                        member.MembershipStatus = storedStatus;
                    }
                }
                else
                {
                    member.MembershipStatus = "Inactive";
                }

                // Filter by status if specified
                if (!string.IsNullOrWhiteSpace(statusFilter) && !statusFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    if (!member.MembershipStatus.Equals(statusFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                list.Add(member);
            }

            return list;
        }

        public Member? GetById(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    m.member_id, 
                    m.name, 
                    m.date_of_birth, 
                    m.gender, 
                    m.contact, 
                    m.address, 
                    m.registration_date, 
                    m.assigned_trainer_id,
                    t.name AS trainer_name,
                    p.plan_name,
                    mem_ship.expiry_date,
                    mem_ship.status
                FROM members m
                LEFT JOIN trainers t ON m.assigned_trainer_id = t.trainer_id
                LEFT JOIN (
                    SELECT m1.member_id, m1.plan_id, m1.expiry_date, m1.status
                    FROM memberships m1
                    INNER JOIN (
                        SELECT member_id, MAX(expiry_date) as max_exp
                        FROM memberships
                        GROUP BY member_id
                    ) m2 ON m1.member_id = m2.member_id AND m1.expiry_date = m2.max_exp
                ) mem_ship ON m.member_id = mem_ship.member_id
                LEFT JOIN membership_plans p ON mem_ship.plan_id = p.plan_id
                WHERE m.member_id = @id;
            ";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var member = new Member
                {
                    MemberId = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    DateOfBirth = reader.IsDBNull(2) ? null : DateTime.Parse(reader.GetString(2)),
                    Gender = reader.GetString(3),
                    Contact = reader.GetString(4),
                    Address = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    RegistrationDate = DateTime.Parse(reader.GetString(6)),
                    AssignedTrainerId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    AssignedTrainerName = reader.IsDBNull(8) ? "None" : reader.GetString(8),
                    CurrentPlanName = reader.IsDBNull(9) ? "None" : reader.GetString(9)
                };

                if (!reader.IsDBNull(10))
                {
                    var expDate = DateTime.Parse(reader.GetString(10));
                    member.CurrentExpiryDate = expDate;
                    var storedStatus = reader.GetString(11);
                    member.MembershipStatus = (storedStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) && expDate >= DateTime.Today)
                        ? "Active" : (storedStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) ? "Expired" : storedStatus);
                }
                else
                {
                    member.MembershipStatus = "Inactive";
                }
                return member;
            }
            return null;
        }

        public int Add(Member member)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO members (name, date_of_birth, gender, contact, address, registration_date, assigned_trainer_id)
                VALUES (@name, @dob, @gender, @contact, @address, @regDate, @trainerId);
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("@name", member.FullName);
            cmd.Parameters.AddWithValue("@dob", member.DateOfBirth.HasValue ? member.DateOfBirth.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@gender", member.Gender);
            cmd.Parameters.AddWithValue("@contact", member.Contact);
            cmd.Parameters.AddWithValue("@address", member.Address ?? "");
            cmd.Parameters.AddWithValue("@regDate", member.RegistrationDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@trainerId", member.AssignedTrainerId.HasValue ? member.AssignedTrainerId.Value : (object)DBNull.Value);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool Update(Member member)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE members 
                SET name = @name, date_of_birth = @dob, gender = @gender, contact = @contact, 
                    address = @address, assigned_trainer_id = @trainerId
                WHERE member_id = @id;
            ";
            cmd.Parameters.AddWithValue("@id", member.MemberId);
            cmd.Parameters.AddWithValue("@name", member.FullName);
            cmd.Parameters.AddWithValue("@dob", member.DateOfBirth.HasValue ? member.DateOfBirth.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@gender", member.Gender);
            cmd.Parameters.AddWithValue("@contact", member.Contact);
            cmd.Parameters.AddWithValue("@address", member.Address ?? "");
            cmd.Parameters.AddWithValue("@trainerId", member.AssignedTrainerId.HasValue ? member.AssignedTrainerId.Value : (object)DBNull.Value);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM members WHERE member_id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool AssignTrainer(int memberId, int? trainerId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE members SET assigned_trainer_id = @trainerId WHERE member_id = @id;";
            cmd.Parameters.AddWithValue("@id", memberId);
            cmd.Parameters.AddWithValue("@trainerId", trainerId.HasValue ? trainerId.Value : (object)DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
