using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymManagementSystem.Data;
using GymManagementSystem.Models;

namespace GymManagementSystem.Data.Repositories
{
    public class AttendanceRepository
    {
        public int RecordAttendance(int memberId, string? notes = null, DateTime? date = null, string? checkInTime = null)
        {
            var attDate = date ?? DateTime.Today;
            var timeStr = checkInTime ?? DateTime.Now.ToString("hh:mm tt");

            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO attendance (member_id, attendance_date, check_in_time, notes)
                VALUES (@memberId, @attDate, @time, @notes);
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("@memberId", memberId);
            cmd.Parameters.AddWithValue("@attDate", attDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@time", timeStr);
            cmd.Parameters.AddWithValue("@notes", notes ?? "");

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool IsAlreadyCheckedInToday(int memberId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM attendance WHERE member_id = @id AND attendance_date = @today;";
            cmd.Parameters.AddWithValue("@id", memberId);
            cmd.Parameters.AddWithValue("@today", DateTime.Today.ToString("yyyy-MM-dd"));
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public List<Attendance> GetTodayAttendance()
        {
            return SearchAttendance(null, DateTime.Today);
        }

        public List<Attendance> SearchAttendance(string? memberQuery = null, DateTime? date = null)
        {
            var list = new List<Attendance>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT a.attendance_id, a.member_id, m.name, a.attendance_date, a.check_in_time, a.notes
                FROM attendance a
                INNER JOIN members m ON a.member_id = m.member_id
                WHERE 1=1
            ";

            if (date.HasValue)
            {
                cmd.CommandText += " AND a.attendance_date = @date";
                cmd.Parameters.AddWithValue("@date", date.Value.ToString("yyyy-MM-dd"));
            }

            if (!string.IsNullOrWhiteSpace(memberQuery))
            {
                cmd.CommandText += " AND (m.name LIKE @q OR printf('MEM-%04d', m.member_id) LIKE @q OR CAST(m.member_id AS TEXT) LIKE @q)";
                cmd.Parameters.AddWithValue("@q", $"%{memberQuery.Trim()}%");
            }

            cmd.CommandText += " ORDER BY a.attendance_id DESC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Attendance
                {
                    AttendanceId = reader.GetInt32(0),
                    MemberId = reader.GetInt32(1),
                    MemberName = reader.GetString(2),
                    AttendanceDate = DateTime.Parse(reader.GetString(3)),
                    CheckInTime = reader.GetString(4),
                    Notes = reader.IsDBNull(5) ? "" : reader.GetString(5)
                });
            }
            return list;
        }

        public List<Attendance> GetMemberAttendanceHistory(int memberId)
        {
            var list = new List<Attendance>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT a.attendance_id, a.member_id, m.name, a.attendance_date, a.check_in_time, a.notes
                FROM attendance a
                INNER JOIN members m ON a.member_id = m.member_id
                WHERE a.member_id = @id
                ORDER BY a.attendance_date DESC, a.attendance_id DESC;
            ";
            cmd.Parameters.AddWithValue("@id", memberId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Attendance
                {
                    AttendanceId = reader.GetInt32(0),
                    MemberId = reader.GetInt32(1),
                    MemberName = reader.GetString(2),
                    AttendanceDate = DateTime.Parse(reader.GetString(3)),
                    CheckInTime = reader.GetString(4),
                    Notes = reader.IsDBNull(5) ? "" : reader.GetString(5)
                });
            }
            return list;
        }

        public bool Delete(int attendanceId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM attendance WHERE attendance_id = @id;";
            cmd.Parameters.AddWithValue("@id", attendanceId);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
