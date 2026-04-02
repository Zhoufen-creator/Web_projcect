using DoAnWeb.Models;
using DoAnWeb.Services;

namespace DoAnWeb.Areas.Admin.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }

        public int PendingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }

        public List<Appointment> RecentAppointments { get; set; } = new();

        // SỬA: insight tải khám theo khoa
        public List<SpecialtyLoadInsight> SpecialtyLoadInsights { get; set; } = new();
    }
}