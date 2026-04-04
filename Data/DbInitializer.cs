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
            string[] defaultSpecialties =
            {
                "Noi tong quat",
                "Tieu hoa",
                "Tim mach",
                "Da lieu",
                "Mat",
                "Tai mui hong",
                "Xuong khop"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            foreach (var specialtyName in defaultSpecialties)
            {
                if (!await context.Specialties.AnyAsync(s => s.Name == specialtyName))
                {
                    context.Specialties.Add(new Specialty
                    {
                        Name = specialtyName,
                        AveragePatientLoad = 0,
                        MaxPatientsPerWeek = 100
                    });
                }
            }

            await context.SaveChangesAsync();

            var defaultSpecialtyId = await context.Specialties
                .Where(s => s.Name == "Noi tong quat")
                .Select(s => s.Id)
                .FirstAsync();

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
                    Name = "Bac si mau",
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

                if (doctorProfile == null)
                {
                    context.Doctors.Add(new Doctor
                    {
                        UserId = doctorUser.Id,
                        SpecialtyId = defaultSpecialtyId,
                        LicenseNumber = "BS001",
                        Qualifications = "Bac si da khoa"
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
                    Name = "Nhan vien mau",
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
                    Name = "Benh nhan mau",
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
