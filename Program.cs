using DoAnWeb.Data;
using DoAnWeb.Models;
using DoAnWeb.Services;
using DoAnWeb.Services.Interface;
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.Configure<PhoBertApiOptions>(builder.Configuration.GetSection("PhoBertApi"));
builder.Services.AddHttpClient<IPhoBertInferenceService, PhoBertInferenceService>();
builder.Services.AddScoped<IDoctorAutoAssignmentService, DoctorAutoAssignmentService>();
builder.Services.AddScoped<ISpecialtyLoadAnalysisService, SpecialtyLoadAnalysisService>();
builder.Services.AddScoped<IStaffingService, StaffingService>();
builder.Services.Configure<SeasonalAnomalyDetectionOptions>(builder.Configuration.GetSection("SeasonalAnomalyDetection"));
builder.Services.AddScoped<ISeasonalStaffingDetectionService, SeasonalStaffingDetectionService>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<AppointmentReminderBackgroundService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

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

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await DbInitializer.SeedRolesAndAdminAsync(services);
}

app.Run();
