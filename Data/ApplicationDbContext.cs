using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DoAnWeb.Models; // Đảm bảo namespace này khớp với các file Model của bạn

namespace DoAnWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        // Nhóm Người dùng & Hồ sơ
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Employee> Employees { get; set; }

        // Nhóm Lịch trình & Khám bệnh
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalExamination> MedicalExaminations { get; set; }

        // Nhóm Thuốc & Dịch vụ
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<MedicalService> MedicalServices { get; set; }
        public DbSet<ExaminationService> ExaminationServices { get; set; }

        // Nhóm Hệ thống (Thông báo, Mail, Log)
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<EmailHistory> EmailHistories { get; set; }
        public DbSet<SiteVisit> SiteVisits { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Cấu hình tên bảng cho Identity (Theo hướng chuyên nghiệp)
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // 2. Thiết lập quan hệ 1-1 (User <-> Patient/Doctor/Employee)
            builder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserId);

            builder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId);

            builder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId);

            // 3. XỬ LÝ LỖI CASCADE DELETE (Cực kỳ quan trọng)
            // Lặp qua tất cả các khóa ngoại và chuyển chế độ xóa từ Cascade sang Restrict
            // Điều này ngăn SQL Server báo lỗi khi có nhiều đường dẫn xóa trùng nhau
            var cascadeFKs = builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // 4. Cấu hình bổ sung (Nếu cần thiết)
            // Ví dụ: Thiết lập độ chính xác cho cột giá tiền (Price)
            builder.Entity<Medicine>()
                .Property(m => m.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<MedicalService>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}