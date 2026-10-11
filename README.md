# Gym Management System (.NET C# WPF Desktop Application)

A modern, desktop-based application developed in **C# .NET 8 (WPF)** with **SQLite** for managing gym operations, memberships, trainers, attendance check-ins, payment transactions, and reporting.

---

## 🚀 Quick Start & How to Run

### 1. Requirements
- Windows 10 / 11
- .NET 8.0 SDK (or later)

### 2. Run the Application
Open PowerShell or Command Prompt in the project directory:
```powershell
dotnet run --project GymManagementSystem
```
Or open `GymManagementSystem.sln` in **Visual Studio 2022** and press **F5** (Start Debugging).

### 3. Run Automated Tests
```powershell
dotnet test
```

---

## 🔑 Default Login Credentials

The system initializes automatically on first run and seeds the following staff accounts:

| Role | Username | Password | Access Level |
| :--- | :--- | :--- | :--- |
| **System Administrator** | `admin` | `admin123` | Full access (Dashboard, Members, Memberships, Trainers, Attendance, Payments, Reports, Staff Management & Database Backup) |
| **Front Desk Staff** | `staff` | `staff123` | Operational access (Dashboard, Members, Memberships, Trainers, Attendance, Payments, Reports) |

*All passwords are encrypted with SHA-256 and salted.*

---

## 📦 System Modules & Features

### 1. Staff Authentication & Access Control
- Secure username and password login window with validation feedback.
- Role-based navigation: Administrator vs. Front Desk Staff.
- Staff Account Management: Administrators can add new staff, update existing staff, and change credentials.

### 2. Member Management
- **Member Registration**: Captures Full Name, Gender, Contact Number, Date of Birth, Address, Registration Date, and optional Assigned Trainer.
- **Search & Filtering**: Instant search by Member ID (`MEM-0001`) or Name, and filter by membership status (`Active`, `Expired`, `Inactive`).
- **Update & Delete**: In-place member editing and deletion with data safety prompts.
- **Assigned Trainer**: Assign personal fitness trainers to members directly.

### 3. Membership & Plans Management
- **Plans Catalog**: Manage gym packages (Monthly Basic, Quarterly Standard, Semi-Annual Pro, Annual Elite) with customized duration (in months) and pricing.
- **Assign Membership**: Assign plans to members with auto-calculated start and expiration dates.
- **Renewals**: 1-click renewal functionality that seamlessly extends the membership past existing expiration dates.
- **Status Tracking**: Visual tracking of active vs expired subscriptions.

### 4. Trainer Management
- **Trainer Directory**: Records Trainer Name, Specialization (e.g., Strength, Cardio, Yoga, CrossFit), Contact, and Schedule Availability (Morning, Evening, Full Day).
- **Search & Edit**: Search trainers by name or specialization; update schedules dynamically.

### 5. Attendance Management
- **Fast Check-In**: Record member arrival at the door with instant membership validity verification.
- **Door Alerts**: Alerts staff if a member's plan has expired or if they have already checked in today.
- **Attendance Log & History**: Filter attendance records by member or date, and view full visit histories.

### 6. Payment Processing & Receipt Generation
- **Fee Calculation**: Automatically populates plan prices when recording payments.
- **Payment Methods & Statuses**: Supports Cash, Credit Card, Debit Card, and Bank Transfer across `Paid`, `Pending`, and `Overdue` statuses.
- **Receipt Generation**: Produces formatted official receipts with receipt numbering (`REC-YYYY-XXXX`), member details, itemized breakdown, and cashier signature.
- **Print & Export**: Print receipts directly or export them as `.txt` files.

### 7. Operations Dashboard & Reports
- **Real-Time KPI Cards**: Total Members, Active Plans, Expired Plans, Today's Check-Ins, and Total Revenue.
- **Quick Action Bar**: Shortcuts for common front-desk tasks.
- **Summary Reports**:
  - Membership Status Distribution (counts and percentages).
  - Financial Collections Summary (Paid, Pending, Overdue, and breakdown by payment method).
  - Attendance Utilization (total visits, unique members).
  - One-click export of complete summary reports to text files.

### 8. Database & Reliability
- **Zero-Setup SQLite Engine**: Standalone local database (`gym_management.db`) stored in Local AppData.
- **Database Backup**: Administrators can back up the database to any destination directory directly from the interface.

---

## 🗄️ Database Schema

- `staff` (`staff_id`, `name`, `username`, `password_hash`, `role`, `contact`, `created_at`)
- `members` (`member_id`, `name`, `date_of_birth`, `gender`, `contact`, `address`, `registration_date`, `assigned_trainer_id`)
- `membership_plans` (`plan_id`, `plan_name`, `duration_months`, `price`, `description`)
- `memberships` (`membership_id`, `member_id`, `plan_id`, `start_date`, `expiry_date`, `status`, `notes`)
- `trainers` (`trainer_id`, `name`, `specialization`, `contact`, `availability`)
- `attendance` (`attendance_id`, `member_id`, `attendance_date`, `check_in_time`, `notes`)
- `payments` (`payment_id`, `member_id`, `membership_id`, `amount`, `payment_date`, `payment_method`, `status`, `receipt_number`, `notes`)
