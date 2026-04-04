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

            if (!await context.Specialties.AnyAsync())
            {
                context.Specialties.AddRange(
                    new Specialty { Name = "Noi tong quat", AveragePatientLoad = 12, MaxPatientsPerWeek = 100 },
                    new Specialty { Name = "Tai mui hong", AveragePatientLoad = 10, MaxPatientsPerWeek = 90 },
                    new Specialty { Name = "Tim mach", AveragePatientLoad = 8, MaxPatientsPerWeek = 80 },
                    new Specialty { Name = "Da lieu", AveragePatientLoad = 9, MaxPatientsPerWeek = 85 }
                );

                await context.SaveChangesAsync();
            }

            var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    Name = "Quan tri vien",
                    Gender = "Khac",
                    CreateAt = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

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
            else if (!await userManager.IsInRoleAsync(doctorUser, "Doctor"))
            {
                await userManager.AddToRoleAsync(doctorUser, "Doctor");
            }

            if (doctorUser != null)
            {
                var doctorProfile = await context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == doctorUser.Id);

                var existingSpecialtyId = await context.Specialties
                    .OrderBy(s => s.Id)
                    .Select(s => (int?)s.Id)
                    .FirstOrDefaultAsync();

                if (doctorProfile == null && existingSpecialtyId.HasValue)
                {
                    context.Doctors.Add(new Doctor
                    {
                        UserId = doctorUser.Id,
                        SpecialtyId = existingSpecialtyId.Value,
                        LicenseNumber = "BS001",
                        Qualifications = "Bác sĩ đa khoa"
                    });

                    await context.SaveChangesAsync();
                }
            }

            var employeeUser = await userManager.FindByEmailAsync("employee@gmail.com");
            if (employeeUser == null)
            {
                employeeUser = new ApplicationUser
                {
                    UserName = "employee@gmail.com",
                    Email = "employee@gmail.com",
                    Name = "Nhân viên mẫu",
                    Gender = "Nu",
                    CreateAt = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(employeeUser, "Employee@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(employeeUser, "Employee");
                }
            }
            else if (!await userManager.IsInRoleAsync(employeeUser, "Employee"))
            {
                await userManager.AddToRoleAsync(employeeUser, "Employee");
            }

            var patientUser = await userManager.FindByEmailAsync("patient@gmail.com");
            if (patientUser == null)
            {
                patientUser = new ApplicationUser
                {
                    UserName = "patient@gmail.com",
                    Email = "patient@gmail.com",
                    Name = "Bệnh nhân mẫu",
                    Gender = "Nu",
                    CreateAt = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(patientUser, "Patient@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(patientUser, "Patient");
                }
            }
            else if (!await userManager.IsInRoleAsync(patientUser, "Patient"))
            {
                await userManager.AddToRoleAsync(patientUser, "Patient");
            }
        }
    }
}
