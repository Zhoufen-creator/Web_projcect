namespace DoAnWeb.Services.Interface
{
    public interface IAppointmentValidationService
    {
        Task<(bool IsValid, List<string> Errors)> ValidateAppointmentAsync(
            int doctorId,
            DateTime scheduledDate,
            int? excludeAppointmentId = null);
    }
}
