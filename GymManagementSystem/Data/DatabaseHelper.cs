using System;
using System.IO;
using Microsoft.Data.Sqlite;
using GymManagementSystem.Services;

namespace GymManagementSystem.Data
{
    public static class DatabaseHelper
    {
        private static string _customDbPath = string.Empty;

        public static string DatabasePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_customDbPath))
                {
                    return _customDbPath;
                }
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GymManagementSystem");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                return Path.Combine(folder, "gym_management.db");
            }
        }

        public static void SetCustomDatabasePath(string path)
        {
            _customDbPath = path;
        }

        public static string ConnectionString => $"Data Source={DatabasePath}";

        public static SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static void InitializeDatabase()
        {
            using var conn = GetConnection();
            using var transaction = conn.BeginTransaction();

            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            cmd.CommandText = @"
                PRAGMA foreign_keys = ON;

                CREATE TABLE IF NOT EXISTS staff (
                    staff_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    username TEXT NOT NULL UNIQUE,
                    password_hash TEXT NOT NULL,
                    role TEXT NOT NULL DEFAULT 'Staff',
                    contact TEXT,
                    created_at TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS trainers (
                    trainer_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    specialization TEXT NOT NULL,
                    contact TEXT NOT NULL,
                    availability TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS membership_plans (
                    plan_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    plan_name TEXT NOT NULL,
                    duration_months INTEGER NOT NULL,
                    price REAL NOT NULL,
                    description TEXT
                );

                CREATE TABLE IF NOT EXISTS members (
                    member_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    date_of_birth TEXT,
                    gender TEXT NOT NULL,
                    contact TEXT NOT NULL,
                    address TEXT,
                    registration_date TEXT NOT NULL,
                    assigned_trainer_id INTEGER,
                    FOREIGN KEY(assigned_trainer_id) REFERENCES trainers(trainer_id) ON DELETE SET NULL
                );

                CREATE TABLE IF NOT EXISTS memberships (
                    membership_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    member_id INTEGER NOT NULL,
                    plan_id INTEGER NOT NULL,
                    start_date TEXT NOT NULL,
                    expiry_date TEXT NOT NULL,
                    status TEXT NOT NULL DEFAULT 'Active',
                    notes TEXT,
                    FOREIGN KEY(member_id) REFERENCES members(member_id) ON DELETE CASCADE,
                    FOREIGN KEY(plan_id) REFERENCES membership_plans(plan_id) ON DELETE RESTRICT
                );

                CREATE TABLE IF NOT EXISTS attendance (
                    attendance_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    member_id INTEGER NOT NULL,
                    attendance_date TEXT NOT NULL,
                    check_in_time TEXT NOT NULL,
                    notes TEXT,
                    FOREIGN KEY(member_id) REFERENCES members(member_id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS payments (
                    payment_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    member_id INTEGER NOT NULL,
                    membership_id INTEGER,
                    amount REAL NOT NULL,
                    payment_date TEXT NOT NULL,
                    payment_method TEXT NOT NULL,
                    status TEXT NOT NULL DEFAULT 'Paid',
                    receipt_number TEXT NOT NULL,
                    notes TEXT,
                    FOREIGN KEY(member_id) REFERENCES members(member_id) ON DELETE CASCADE,
                    FOREIGN KEY(membership_id) REFERENCES memberships(membership_id) ON DELETE SET NULL
                );
            ";
            cmd.ExecuteNonQuery();
            transaction.Commit();

            SeedDefaultData(conn);
        }

        private static void SeedDefaultData(SqliteConnection conn)
        {
            // Seed staff accounts if empty
            using var countCmd = conn.CreateCommand();
            countCmd.CommandText = "SELECT COUNT(*) FROM staff;";
            var staffCount = Convert.ToInt32(countCmd.ExecuteScalar());

            if (staffCount == 0)
            {
                using var insertStaff = conn.CreateCommand();
                insertStaff.CommandText = @"
                    INSERT INTO staff (name, username, password_hash, role, contact, created_at)
                    VALUES 
                    (@adminName, 'admin', @adminPass, 'Admin', '0752147143', @today),
                    (@staffName, 'staff', @staffPass, 'Staff', '0724147143', @today);
                ";
                insertStaff.Parameters.AddWithValue("@adminName", "System Administrator");
                insertStaff.Parameters.AddWithValue("@adminPass", AuthService.HashPassword("admin123"));
                insertStaff.Parameters.AddWithValue("@staffName", "Front Desk Receptionist");
                insertStaff.Parameters.AddWithValue("@staffPass", AuthService.HashPassword("staff123"));
                insertStaff.Parameters.AddWithValue("@today", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                insertStaff.ExecuteNonQuery();
            }

            // Seed membership plans if empty
            using var planCountCmd = conn.CreateCommand();
            planCountCmd.CommandText = "SELECT COUNT(*) FROM membership_plans;";
            var planCount = Convert.ToInt32(planCountCmd.ExecuteScalar());

            if (planCount == 0)
            {
                using var insertPlans = conn.CreateCommand();
                insertPlans.CommandText = @"
                    INSERT INTO membership_plans (plan_name, duration_months, price, description)
                    VALUES 
                    ('Monthly Basic', 1, 45.00, 'Standard gym access for 1 month'),
                    ('Quarterly Standard', 3, 120.00, '3 months gym access with free locker'),
                    ('Semi-Annual Pro', 6, 210.00, '6 months gym access with locker & fitness consultation'),
                    ('Annual Elite', 12, 380.00, 'Full 12 months access with all amenities and personal assessment');
                ";
                insertPlans.ExecuteNonQuery();
            }

            // Seed trainers if empty
            using var trainerCountCmd = conn.CreateCommand();
            trainerCountCmd.CommandText = "SELECT COUNT(*) FROM trainers;";
            var trainerCount = Convert.ToInt32(trainerCountCmd.ExecuteScalar());

            if (trainerCount == 0)
            {
                using var insertTrainers = conn.CreateCommand();
                insertTrainers.CommandText = @"
                    INSERT INTO trainers (name, specialization, contact, availability)
                    VALUES 
                    ('Captain Miller', 'Strength & Powerlifting', '555-0301', 'Morning (6 AM - 12 PM)'),
                    ('VIP Raguvaran', 'Cardio & HIIT Conditioning', '555-0302', 'Evening (3 PM - 9 PM)'),
                    ('Sinna Chami', 'CrossFit & Functional Training', '555-0303', 'Full Day');
                ";
                insertTrainers.ExecuteNonQuery();
            }

            // Seed sample members if empty
            using var memberCountCmd = conn.CreateCommand();
            memberCountCmd.CommandText = "SELECT COUNT(*) FROM members;";
            var memberCount = Convert.ToInt32(memberCountCmd.ExecuteScalar());

            if (memberCount == 0)
            {
                var today = DateTime.Today;
                using var insertMembers = conn.CreateCommand();
                insertMembers.CommandText = @"
                    INSERT INTO members (name, date_of_birth, gender, contact, address, registration_date, assigned_trainer_id)
                    VALUES 
                    ('David billa', '1995-04-12', 'Male', '555-1101', '742 Evergreen Terrace', @reg1, 1),
                    ('Kayal vizhi', '1998-08-25', 'Female', '555-1102', '12 Grimmauld Place', @reg2, 2),
                    ('Sachin', '1985-03-15', 'Male', '555-1103', '1725 Slough Avenue', @reg3, 3),
                    ('Sweshthika', '2001-11-09', 'Female', '555-1104', '404 Baker Street', @reg4, 1);
                ";
                insertMembers.Parameters.AddWithValue("@reg1", today.AddMonths(-2).ToString("yyyy-MM-dd"));
                insertMembers.Parameters.AddWithValue("@reg2", today.AddMonths(-1).ToString("yyyy-MM-dd"));
                insertMembers.Parameters.AddWithValue("@reg3", today.AddMonths(-4).ToString("yyyy-MM-dd"));
                insertMembers.Parameters.AddWithValue("@reg4", today.AddDays(-10).ToString("yyyy-MM-dd"));
                insertMembers.ExecuteNonQuery();

                // Seed memberships
                using var insertMemberships = conn.CreateCommand();
                insertMemberships.CommandText = @"
                    INSERT INTO memberships (member_id, plan_id, start_date, expiry_date, status, notes)
                    VALUES 
                    (1, 2, @m1Start, @m1Exp, 'Active', 'Quarterly subscription'),
                    (2, 1, @m2Start, @m2Exp, 'Active', 'Monthly renewal'),
                    (3, 1, @m3Start, @m3Exp, 'Expired', 'Membership expired last month'),
                    (4, 4, @m4Start, @m4Exp, 'Active', 'Annual Elite VIP member');
                ";
                insertMemberships.Parameters.AddWithValue("@m1Start", today.AddMonths(-1).ToString("yyyy-MM-dd"));
                insertMemberships.Parameters.AddWithValue("@m1Exp", today.AddMonths(2).ToString("yyyy-MM-dd"));
                insertMemberships.Parameters.AddWithValue("@m2Start", today.AddDays(-15).ToString("yyyy-MM-dd"));
                insertMemberships.Parameters.AddWithValue("@m2Exp", today.AddDays(15).ToString("yyyy-MM-dd"));
                insertMemberships.Parameters.AddWithValue("@m3Start", today.AddMonths(-4).ToString("yyyy-MM-dd"));
                insertMemberships.Parameters.AddWithValue("@m3Exp", today.AddMonths(-3).ToString("yyyy-MM-dd"));
                insertMemberships.Parameters.AddWithValue("@m4Start", today.AddDays(-10).ToString("yyyy-MM-dd"));
                insertMemberships.Parameters.AddWithValue("@m4Exp", today.AddMonths(12).AddDays(-10).ToString("yyyy-MM-dd"));
                insertMemberships.ExecuteNonQuery();

                // Seed Attendance for today
                using var insertAttendance = conn.CreateCommand();
                insertAttendance.CommandText = @"
                    INSERT INTO attendance (member_id, attendance_date, check_in_time, notes)
                    VALUES 
                    (1, @today, '07:15 AM', 'Cardio routine'),
                    (2, @today, '08:30 AM', 'Yoga session'),
                    (4, @today, '09:05 AM', 'Weight lifting');
                ";
                insertAttendance.Parameters.AddWithValue("@today", today.ToString("yyyy-MM-dd"));
                insertAttendance.ExecuteNonQuery();

                // Seed Payments
                using var insertPayments = conn.CreateCommand();
                insertPayments.CommandText = @"
                    INSERT INTO payments (member_id, membership_id, amount, payment_date, payment_method, status, receipt_number, notes)
                    VALUES 
                    (1, 1, 120.00, @p1Date, 'Credit Card', 'Paid', 'REC-2026-0001', 'Quarterly fee'),
                    (2, 2, 45.00, @p2Date, 'Cash', 'Paid', 'REC-2026-0002', 'Monthly fee'),
                    (3, 3, 45.00, @p3Date, 'Cash', 'Paid', 'REC-2026-0003', 'Initial month fee'),
                    (4, 4, 380.00, @p4Date, 'Credit Card', 'Paid', 'REC-2026-0004', 'Annual VIP package');
                ";
                insertPayments.Parameters.AddWithValue("@p1Date", today.AddMonths(-1).ToString("yyyy-MM-dd"));
                insertPayments.Parameters.AddWithValue("@p2Date", today.AddDays(-15).ToString("yyyy-MM-dd"));
                insertPayments.Parameters.AddWithValue("@p3Date", today.AddMonths(-4).ToString("yyyy-MM-dd"));
                insertPayments.Parameters.AddWithValue("@p4Date", today.AddDays(-10).ToString("yyyy-MM-dd"));
                insertPayments.ExecuteNonQuery();
            }
        }

        public static bool BackupDatabase(string destinationPath)
        {
            try
            {
                using var conn = GetConnection();
                // Checkpoint WAL if any
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA wal_checkpoint(FULL);";
                cmd.ExecuteNonQuery();

                File.Copy(DatabasePath, destinationPath, true);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
