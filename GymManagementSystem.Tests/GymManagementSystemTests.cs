using System;
using System.IO;
using Xunit;
using GymManagementSystem.Data;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Models;
using GymManagementSystem.Services;

namespace GymManagementSystem.Tests
{
    public class GymManagementSystemTests : IDisposable
    {
        private readonly string _testDbPath;

        public GymManagementSystemTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"gym_test_{Guid.NewGuid():N}.db");
            DatabaseHelper.SetCustomDatabasePath(_testDbPath);
            DatabaseHelper.InitializeDatabase();
        }

        public void Dispose()
        {
            try
            {
                if (File.Exists(_testDbPath))
                {
                    File.Delete(_testDbPath);
                }
            }
            catch
            {
                // Ignore cleanup lock if still releasing
            }
        }

        [Fact]
        public void AuthService_HashAndVerify_Success()
        {
            var rawPass = "Secret123!";
            var hash = AuthService.HashPassword(rawPass);

            Assert.NotEmpty(hash);
            Assert.NotEqual(rawPass, hash);
            Assert.True(AuthService.VerifyPassword(rawPass, hash));
            Assert.False(AuthService.VerifyPassword("WrongPass", hash));
        }

        [Fact]
        public void StaffRepository_AuthenticateDefaultAdmin_Success()
        {
            var repo = new StaffRepository();
            var admin = repo.Authenticate("admin", "admin123");

            Assert.NotNull(admin);
            Assert.Equal("admin", admin.Username);
            Assert.Equal("Admin", admin.Role);

            var staff = repo.Authenticate("staff", "staff123");
            Assert.NotNull(staff);
            Assert.Equal("staff", staff.Username);
            Assert.Equal("Staff", staff.Role);

            var invalid = repo.Authenticate("admin", "wrong_password");
            Assert.Null(invalid);
        }

        [Fact]
        public void StaffRepository_CreateUpdateDelete_Success()
        {
            var repo = new StaffRepository();
            var newStaff = new Staff
            {
                FullName = "Test Assistant",
                Username = "testasst",
                Role = "Staff",
                Contact = "555-9999"
            };

            int id = repo.Add(newStaff, "myPass123");
            Assert.True(id > 0);

            var retrieved = repo.GetById(id);
            Assert.NotNull(retrieved);
            Assert.Equal("Test Assistant", retrieved.FullName);

            // Update
            retrieved.FullName = "Updated Assistant";
            bool updated = repo.Update(retrieved, "newPass456");
            Assert.True(updated);

            var reAuth = repo.Authenticate("testasst", "newPass456");
            Assert.NotNull(reAuth);

            // Delete
            bool deleted = repo.Delete(id);
            Assert.True(deleted);
            Assert.Null(repo.GetById(id));
        }

        [Fact]
        public void MemberRepository_AddUpdateSearch_Success()
        {
            var repo = new MemberRepository();
            var newMember = new Member
            {
                FullName = "Alice Cooper",
                Gender = "Female",
                Contact = "555-4321",
                Address = "100 Rock Rd",
                RegistrationDate = DateTime.Today,
                DateOfBirth = new DateTime(1996, 5, 20)
            };

            int memberId = repo.Add(newMember);
            Assert.True(memberId > 0);

            var retrieved = repo.GetById(memberId);
            Assert.NotNull(retrieved);
            Assert.Equal("Alice Cooper", retrieved.FullName);
            Assert.Equal("MEM-" + memberId.ToString("D4"), retrieved.DisplayId);

            // Search by name
            var searchResults = repo.GetAll("Alice");
            Assert.Contains(searchResults, m => m.MemberId == memberId);

            // Update
            retrieved.Contact = "555-8888";
            Assert.True(repo.Update(retrieved));

            var updatedMem = repo.GetById(memberId);
            Assert.Equal("555-8888", updatedMem?.Contact);
        }

        [Fact]
        public void MembershipRepository_AssignAndRenew_Success()
        {
            var memberRepo = new MemberRepository();
            var membershipRepo = new MembershipRepository();

            var memberId = memberRepo.Add(new Member
            {
                FullName = "Robert Plant",
                Gender = "Male",
                Contact = "555-7777",
                RegistrationDate = DateTime.Today
            });

            var plans = membershipRepo.GetAllPlans();
            Assert.NotEmpty(plans);
            var plan = plans[0]; // e.g. Monthly Basic (1 month)

            var startDate = DateTime.Today;
            int membershipId = membershipRepo.AssignMembership(memberId, plan.PlanId, startDate, "Initial signup");
            Assert.True(membershipId > 0);

            var latest = membershipRepo.GetLatestMembershipByMemberId(memberId);
            Assert.NotNull(latest);
            Assert.Equal(plan.PlanName, latest.PlanName);
            Assert.Equal(startDate.AddMonths(plan.DurationInMonths).ToString("yyyy-MM-dd"), latest.ExpiryDate.ToString("yyyy-MM-dd"));

            // Renew membership
            bool renewed = membershipRepo.RenewMembership(memberId, plan.PlanId, "Renewed for next period");
            Assert.True(renewed);

            var renewedLatest = membershipRepo.GetLatestMembershipByMemberId(memberId);
            Assert.NotNull(renewedLatest);
            // Renewal should extend past the previous expiry date
            Assert.True(renewedLatest.ExpiryDate > latest.ExpiryDate);
        }

        [Fact]
        public void AttendanceRepository_RecordAndQuery_Success()
        {
            var memberRepo = new MemberRepository();
            var attRepo = new AttendanceRepository();

            var memberId = memberRepo.Add(new Member
            {
                FullName = "Bruce Wayne",
                Gender = "Male",
                Contact = "555-0007",
                RegistrationDate = DateTime.Today
            });

            int attId = attRepo.RecordAttendance(memberId, "Morning cardio", DateTime.Today, "07:30 AM");
            Assert.True(attId > 0);

            Assert.True(attRepo.IsAlreadyCheckedInToday(memberId));

            var history = attRepo.GetMemberAttendanceHistory(memberId);
            Assert.NotEmpty(history);
            Assert.Equal("07:30 AM", history[0].CheckInTime);
        }

        [Fact]
        public void PaymentRepository_RecordPaymentAndReceipt_Success()
        {
            var memberRepo = new MemberRepository();
            var paymentRepo = new PaymentRepository();

            var memberId = memberRepo.Add(new Member
            {
                FullName = "Clark Kent",
                Gender = "Male",
                Contact = "555-9000",
                RegistrationDate = DateTime.Today
            });

            var member = memberRepo.GetById(memberId);
            var payment = new Payment
            {
                MemberId = memberId,
                MemberName = member!.FullName,
                PlanName = "Monthly Basic",
                Amount = 45.00m,
                PaymentDate = DateTime.Today,
                PaymentMethod = "Cash",
                Status = "Paid",
                Notes = "First month payment"
            };

            int payId = paymentRepo.RecordPayment(payment);
            Assert.True(payId > 0);

            var retrieved = paymentRepo.GetPaymentById(payId);
            Assert.NotNull(retrieved);
            Assert.StartsWith("REC-", retrieved.ReceiptNumber);

            // Test Receipt Generation
            var cashier = new Staff { FullName = "Front Desk Officer" };
            var receiptText = ReceiptService.GenerateReceiptText(retrieved, member, cashier);
            Assert.Contains("TITAN FITNESS & GYM CENTER", receiptText);
            Assert.Contains(retrieved.ReceiptNumber, receiptText);
            Assert.Contains("Clark Kent", receiptText);
            Assert.Contains("$45.00", receiptText);
        }

        [Fact]
        public void ReportService_CalculatesMetricsCorrectly()
        {
            var reportService = new ReportService();
            var stats = reportService.GetDashboardStats();

            Assert.True(stats.TotalMembers >= 4); // Default seeded members
            Assert.True(stats.ActiveMemberships >= 1);
            Assert.True(stats.TodayAttendance >= 1);
            Assert.True(stats.TotalRevenue > 0);

            var statusReport = reportService.GetMembershipStatusReport();
            Assert.NotEmpty(statusReport);

            var financial = reportService.GetFinancialReport(DateTime.Today.AddMonths(-6), DateTime.Today.AddDays(1));
            Assert.True(financial.TotalPaid > 0);
            Assert.True(financial.TransactionCount > 0);
        }

        [Fact]
        public void DatabaseHelper_Backup_Success()
        {
            var backupPath = Path.Combine(Path.GetTempPath(), $"gym_backup_{Guid.NewGuid():N}.db");
            try
            {
                bool success = DatabaseHelper.BackupDatabase(backupPath);
                Assert.True(success);
                Assert.True(File.Exists(backupPath));
                Assert.True(new FileInfo(backupPath).Length > 0);
            }
            finally
            {
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
        }

        [Fact]
        public void ReceiptService_SaveToFile_Success()
        {
            var tempReceipt = Path.Combine(Path.GetTempPath(), $"receipt_{Guid.NewGuid():N}.txt");
            try
            {
                var content = "SAMPLE RECEIPT CONTENT";
                bool saved = ReceiptService.SaveReceiptToFile(tempReceipt, content);
                Assert.True(saved);
                Assert.True(File.Exists(tempReceipt));
                Assert.Equal(content, File.ReadAllText(tempReceipt));
            }
            finally
            {
                if (File.Exists(tempReceipt))
                {
                    File.Delete(tempReceipt);
                }
            }
        }

        [Fact]
        public void StaffRepository_UsernameCollision_Detected()
        {
            var repo = new StaffRepository();
            Assert.True(repo.UsernameExists("admin"));
            Assert.True(repo.UsernameExists("staff"));
            Assert.False(repo.UsernameExists("non_existent_user"));
        }

        [Fact]
        public void MembershipRepository_MultiplePlanDurations_CorrectExpiry()
        {
            var memberRepo = new MemberRepository();
            var membershipRepo = new MembershipRepository();

            var mId = memberRepo.Add(new Member
            {
                FullName = "Duration Test Member",
                Gender = "Female",
                Contact = "555-1234",
                RegistrationDate = DateTime.Today
            });

            int annualPlanId = membershipRepo.AddPlan(new MembershipPlan
            {
                PlanName = "12-Month Plan",
                DurationInMonths = 12,
                Price = 500.00m
            });

            var startDate = new DateTime(2026, 1, 1);
            int memId = membershipRepo.AssignMembership(mId, annualPlanId, startDate);
            Assert.True(memId > 0);

            var latest = membershipRepo.GetLatestMembershipByMemberId(mId);
            Assert.NotNull(latest);
            Assert.Equal(new DateTime(2027, 1, 1), latest.ExpiryDate);
        }

        [Fact]
        public void AttendanceRepository_SearchFilters_Success()
        {
            var memberRepo = new MemberRepository();
            var attRepo = new AttendanceRepository();

            var mId = memberRepo.Add(new Member
            {
                FullName = "UniqueFilterName Member",
                Gender = "Male",
                Contact = "555-8822",
                RegistrationDate = DateTime.Today
            });

            var specificDate = DateTime.Today.AddDays(-2);
            attRepo.RecordAttendance(mId, "Past day test", specificDate, "10:15 AM");

            var results = attRepo.SearchAttendance("UniqueFilterName", specificDate);
            Assert.NotEmpty(results);
            Assert.Equal(mId, results[0].MemberId);
            Assert.Equal("10:15 AM", results[0].CheckInTime);

            var emptyResults = attRepo.SearchAttendance("UniqueFilterName", DateTime.Today.AddDays(-1));
            Assert.Empty(emptyResults);
        }

        [Fact]
        public void MainWindow_Initialization_LoadsDashboardSuccessfully()
        {
            Exception? threadException = null;
            var t = new System.Threading.Thread(() =>
            {
                try
                {
                    if (System.Windows.Application.Current == null)
                    {
                        var app = new System.Windows.Application();
                        app.Resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary
                        {
                            Source = new Uri("/GymManagementSystem;component/Styles/ModernTheme.xaml", UriKind.RelativeOrAbsolute)
                        });
                    }

                    var staffRepo = new StaffRepository();
                    var admin = staffRepo.Authenticate("admin", "admin123");
                    Assert.NotNull(admin);
                    AuthService.SetCurrentUser(admin);

                    var mainWindow = new MainWindow();
                    Assert.NotNull(mainWindow.ContentHost);
                    Assert.NotNull(mainWindow.ContentHost.Content);
                    Assert.IsType<Views.DashboardView>(mainWindow.ContentHost.Content);
                    mainWindow.Close();
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });
            t.SetApartmentState(System.Threading.ApartmentState.STA);
            t.Start();
            t.Join();

            Assert.Null(threadException);
        }
    }
}
