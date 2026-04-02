using DoAnWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "Admin", "Doctor", "Employee", "Patient" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ADMIN
            var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    Name = "Quản trị viên",
                    Gender = "Khác",
                    CreateAt = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // DOCTOR
            var doctorUser = await userManager.FindByEmailAsync("doctor@gmail.com");
            if (doctorUser == null)
            {
                doctorUser = new ApplicationUser
                {
                    UserName = "doctor@gmail.com",
                    Email = "doctor@gmail.com",
                    Name = "Bác sĩ mẫu",
                    Gender = "Nam",
                    CreateAt = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(doctorUser, "Doctor@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(doctorUser, "Doctor");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(doctorUser, "Doctor"))
                {
                    await userManager.AddToRoleAsync(doctorUser, "Doctor");
                }
            }

            // Tạo hồ sơ Doctor nếu chưa có
            if (doctorUser != null)
            {
                var doctorProfile = await context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == doctorUser.Id);

                if (doctorProfile == null)
                {
                    context.Doctors.Add(new Doctor
                    {
                        UserId = doctorUser.Id,
                        Specialty = "Nội tổng quát",
                        LicenseNumber = "BS001",
                        Qualifications = "Bác sĩ đa khoa"
                    });

                    await context.SaveChangesAsync();
                }
            }

            // EMPLOYEE
            var employeeUser = await userManager.FindByEmailAsync("employee@gmail.com");
            if (employeeUser == null)
            {
                employeeUser = new ApplicationUser
                {
                    UserName = "employee@gmail.com",
                    Email = "employee@gmail.com",
                    Name = "Nhân viên mẫu",
                    Gender = "Nữ",
                    CreateAt = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(employeeUser, "Employee@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(employeeUser, "Employee");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(employeeUser, "Employee"))
                {
                    await userManager.AddToRoleAsync(employeeUser, "Employee");
                }
            }

            // PATIENT
            var patientUser = await userManager.FindByEmailAsync("patient@gmail.com");
            if (patientUser == null)
            {
                patientUser = new ApplicationUser
                {
                    UserName = "patient@gmail.com",
                    Email = "patient@gmail.com",
                    Name = "Bệnh nhân mẫu",
                    Gender = "Nữ",
                    CreateAt = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(patientUser, "Patient@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(patientUser, "Patient");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(patientUser, "Patient"))
                {
                    await userManager.AddToRoleAsync(patientUser, "Patient");
                }
            }
        }
    }
}