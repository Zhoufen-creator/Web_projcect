using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DoAnWeb.Models;

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
        public DbSet<Specialty> Specialties { get; set; }

        // Nhóm Lịch trình & Khám bệnh
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalExamination> MedicalExaminations { get; set; }

        // Nhóm Thuốc & Dịch vụ
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<MedicalService> MedicalServices { get; set; }
        public DbSet<ExaminationService> ExaminationServices { get; set; }

        // Nhóm Hệ thống
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<EmailHistory> EmailHistories { get; set; }
        public DbSet<SiteVisit> SiteVisits { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Đổi tên bảng Identity
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<Specialty>().ToTable("Specialties");

            // Quan hệ 1-1: User <-> Patient / Doctor / Employee
            builder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ hệ thống: User -> Notification / EmailHistory / SiteVisit
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EmailHistory>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SiteVisit>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ nghiệp vụ chính

            // Appointment
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // DoctorSchedule
            builder.Entity<DoctorSchedule>()
                .HasOne(ds => ds.Doctor)
                .WithMany(d => d.DoctorSchedules)
                .HasForeignKey(ds => ds.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicalExamination
            builder.Entity<MedicalExamination>()
                .HasOne(m => m.Appointment)
                .WithMany(a => a.MedicalExaminations)
                .HasForeignKey(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MedicalExamination>()
                .HasOne(m => m.Doctor)
                .WithMany(d => d.MedicalExaminations)
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MedicalExamination>()
                .HasOne(m => m.Patient)
                .WithMany(p => p.MedicalExaminations)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription
            builder.Entity<Prescription>()
                .HasOne(p => p.Medicine)
                .WithMany(m => m.Prescriptions)
                .HasForeignKey(p => p.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Prescription>()
                .HasOne(p => p.MedicalExamination)
                .WithMany(m => m.Prescriptions)
                .HasForeignKey(p => p.MedicalExaminationId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExaminationService
            builder.Entity<ExaminationService>()
                .HasOne(es => es.MedicalService)
                .WithMany(ms => ms.ExaminationServices)
                .HasForeignKey(es => es.MedicalServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExaminationService>()
                .HasOne(es => es.Appointment)
                .WithMany(a => a.ExaminationServices)
                .HasForeignKey(es => es.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enum -> string
            builder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasConversion<string>();

            builder.Entity<MedicalExamination>()
                .Property(m => m.Status)
                .HasConversion<string>();

            // Decimal cho giá tiền
            builder.Entity<Medicine>()
                .Property(m => m.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<MedicalService>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            // Chặn lỗi Multiple Cascade Paths
            foreach (var foreignKey in builder.Model.GetEntityTypes()
                         .SelectMany(e => e.GetForeignKeys()))
            {
                if (foreignKey.DeleteBehavior == DeleteBehavior.Cascade)
                {
                    foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }
        }
    }
}