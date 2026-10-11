using System;

namespace GymManagementSystem.Models
{
    public class Staff
    {
        public int StaffId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Staff"; // "Admin" or "Staff"
        public string Contact { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class Member
    {
        public int MemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = "Male"; // "Male", "Female", "Other"
        public string Contact { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.Today;
        public int? AssignedTrainerId { get; set; }
        public string AssignedTrainerName { get; set; } = string.Empty;
        public string MembershipStatus { get; set; } = "Inactive"; // "Active", "Expired", "Inactive"
        public string CurrentPlanName { get; set; } = "None";
        public DateTime? CurrentExpiryDate { get; set; }

        public string DisplayId => $"MEM-{MemberId:D4}";
        public string FormattedDob => DateOfBirth.HasValue ? DateOfBirth.Value.ToString("yyyy-MM-dd") : "N/A";
        public string FormattedRegDate => RegistrationDate.ToString("yyyy-MM-dd");
        public string FormattedExpiry => CurrentExpiryDate.HasValue ? CurrentExpiryDate.Value.ToString("yyyy-MM-dd") : "N/A";
    }

    public class MembershipPlan
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int DurationInMonths { get; set; } = 1;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;

        public string FormattedPrice => $"${Price:F2}";
        public string FormattedDuration => DurationInMonths == 1 ? "1 Month" : $"{DurationInMonths} Months";
    }

    public class MemberMembership
    {
        public int MembershipId { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime ExpiryDate { get; set; } = DateTime.Today.AddMonths(1);
        public string Status { get; set; } = "Active"; // "Active", "Expired", "Inactive"
        public string Notes { get; set; } = string.Empty;

        public string FormattedStartDate => StartDate.ToString("yyyy-MM-dd");
        public string FormattedExpiryDate => ExpiryDate.ToString("yyyy-MM-dd");
        public bool IsActive => Status.Equals("Active", StringComparison.OrdinalIgnoreCase) && ExpiryDate >= DateTime.Today;
    }

    public class Trainer
    {
        public int TrainerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty; // e.g. "Strength & Conditioning", "Cardio & Weight Loss", "Yoga"
        public string Contact { get; set; } = string.Empty;
        public string Availability { get; set; } = "Full Day"; // "Morning (6 AM - 12 PM)", "Evening (3 PM - 9 PM)", "Full Day"

        public string DisplayId => $"TRN-{TrainerId:D3}";
    }

    public class Attendance
    {
        public int AttendanceId { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; } = DateTime.Today;
        public string CheckInTime { get; set; } = DateTime.Now.ToString("hh:mm tt");
        public string Notes { get; set; } = string.Empty;

        public string FormattedDate => AttendanceDate.ToString("yyyy-MM-dd");
        public string DisplayMemberId => $"MEM-{MemberId:D4}";
    }

    public class Payment
    {
        public int PaymentId { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public int? MembershipId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Today;
        public string PaymentMethod { get; set; } = "Cash"; // "Cash", "Credit Card", "Debit Card", "Bank Transfer"
        public string Status { get; set; } = "Paid"; // "Paid", "Pending", "Overdue"
        public string ReceiptNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public string DisplayId => $"PAY-{PaymentId:D5}";
        public string DisplayMemberId => $"MEM-{MemberId:D4}";
        public string FormattedAmount => $"${Amount:F2}";
        public string FormattedDate => PaymentDate.ToString("yyyy-MM-dd");
    }

    public class DashboardStats
    {
        public int TotalMembers { get; set; }
        public int ActiveMemberships { get; set; }
        public int ExpiredMemberships { get; set; }
        public int TodayAttendance { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
