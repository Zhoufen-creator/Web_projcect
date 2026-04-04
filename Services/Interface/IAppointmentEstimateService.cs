using DoAnWeb.Services;

namespace DoAnWeb.Services.Interface
{
    public interface IAppointmentEstimateService
    {
        Task<AppointmentEstimateResult> EstimateAsync(int appointmentId);
    }
}
