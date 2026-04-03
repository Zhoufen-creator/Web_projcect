using DoAnWeb.Data;
using DoAnWeb.Models;
using DoAnWeb.Services;
using DoAnWeb.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddScoped<IAppointmentValidationService, AppointmentValidationService>();
builder.Services.AddScoped<IAppointmentEstimateService, AppointmentEstimateService>();
builder.Services.AddScoped<ISpecialtyPredictionService, SpecialtyPredictionService>();
builder.Services.AddScoped<IDoctorAutoAssignmentService, DoctorAutoAssignmentService>();

// SỬA: thêm service phân tích tải khoa
builder.Services.AddScoped<ISpecialtyLoadAnalysisService, SpecialtyLoadAnalysisService>();

// SỬA: thêm service tính toán nhân sự
builder.Services.AddScoped<IStaffingService, StaffingService>();

// SỬA: thêm service PhoBERT inference
builder.Services.AddScoped<IPhoBertInferenceService, PhoBertInferenceService>();
builder.Services.AddHttpClient<IPhoBertInferenceService, PhoBertInferenceService>();

// Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Reminder background service
builder.Services.AddHostedService<AppointmentReminderBackgroundService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Route cho Area
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.SeedRolesAndAdminAsync(services);
}

app.Run();