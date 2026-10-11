using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymManagementSystem.Data;
using GymManagementSystem.Models;
using GymManagementSystem.Services;

namespace GymManagementSystem.Data.Repositories
{
    public class StaffRepository
    {
        public Staff? Authenticate(string username, string password)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT staff_id, name, username, password_hash, role, contact, created_at FROM staff WHERE username = @username COLLATE NOCASE;";
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var storedHash = reader.GetString(3);
                if (AuthService.VerifyPassword(password, storedHash))
                {
                    return new Staff
                    {
                        StaffId = reader.GetInt32(0),
                        FullName = reader.GetString(1),
                        Username = reader.GetString(2),
                        PasswordHash = storedHash,
                        Role = reader.GetString(4),
                        Contact = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        CreatedAt = DateTime.Parse(reader.GetString(6))
                    };
                }
            }
            return null;
        }

        public List<Staff> GetAll()
        {
            var list = new List<Staff>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT staff_id, name, username, password_hash, role, contact, created_at FROM staff ORDER BY staff_id ASC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Staff
                {
                    StaffId = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Username = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    Role = reader.GetString(4),
                    Contact = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    CreatedAt = DateTime.Parse(reader.GetString(6))
                });
            }
            return list;
        }

        public Staff? GetById(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT staff_id, name, username, password_hash, role, contact, created_at FROM staff WHERE staff_id = @id;";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Staff
                {
                    StaffId = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Username = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    Role = reader.GetString(4),
                    Contact = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    CreatedAt = DateTime.Parse(reader.GetString(6))
                };
            }
            return null;
        }

        public int Add(Staff staff, string plainPassword)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO staff (name, username, password_hash, role, contact, created_at)
                VALUES (@name, @username, @passwordHash, @role, @contact, @createdAt);
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("@name", staff.FullName);
            cmd.Parameters.AddWithValue("@username", staff.Username);
            cmd.Parameters.AddWithValue("@passwordHash", AuthService.HashPassword(plainPassword));
            cmd.Parameters.AddWithValue("@role", staff.Role);
            cmd.Parameters.AddWithValue("@contact", staff.Contact ?? "");
            cmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        public bool Update(Staff staff, string? newPlainPassword = null)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            if (!string.IsNullOrEmpty(newPlainPassword))
            {
                cmd.CommandText = @"
                    UPDATE staff 
                    SET name = @name, username = @username, role = @role, contact = @contact, password_hash = @passwordHash
                    WHERE staff_id = @id;
                ";
                cmd.Parameters.AddWithValue("@passwordHash", AuthService.HashPassword(newPlainPassword));
            }
            else
            {
                cmd.CommandText = @"
                    UPDATE staff 
                    SET name = @name, username = @username, role = @role, contact = @contact
                    WHERE staff_id = @id;
                ";
            }
            cmd.Parameters.AddWithValue("@id", staff.StaffId);
            cmd.Parameters.AddWithValue("@name", staff.FullName);
            cmd.Parameters.AddWithValue("@username", staff.Username);
            cmd.Parameters.AddWithValue("@role", staff.Role);
            cmd.Parameters.AddWithValue("@contact", staff.Contact ?? "");

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM staff WHERE staff_id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UsernameExists(string username, int excludeStaffId = 0)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM staff WHERE username = @username AND staff_id != @excludeId;";
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@excludeId", excludeStaffId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
    }
}
