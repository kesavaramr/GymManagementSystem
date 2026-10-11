using System;
using System.Security.Cryptography;
using System.Text;
using GymManagementSystem.Models;

namespace GymManagementSystem.Services
{
    public static class AuthService
    {
        public static Staff? CurrentStaff { get; private set; }

        public static bool IsLoggedIn => CurrentStaff != null;
        public static bool IsAdmin => CurrentStaff?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

        public static void SetCurrentUser(Staff staff)
        {
            CurrentStaff = staff;
        }

        public static void Logout()
        {
            CurrentStaff = null;
        }

        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            using var sha256 = SHA256.Create();
            var saltedBytes = Encoding.UTF8.GetBytes(password + "GymMS_Salt_2026");
            var hashBytes = sha256.ComputeHash(saltedBytes);
            return Convert.ToHexString(hashBytes);
        }

        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(inputPassword)) return false;
            var inputHash = HashPassword(inputPassword);
            return string.Equals(inputHash, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
