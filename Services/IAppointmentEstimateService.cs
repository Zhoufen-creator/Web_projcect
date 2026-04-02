namespace DoAnWeb.Services
{
    public interface IAppointmentEstimateService
    {
        Task<AppointmentEstimateResult> EstimateAsync(int appointmentId);
    }
}