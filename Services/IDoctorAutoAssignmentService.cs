namespace DoAnWeb.Services
{
    public interface IDoctorAutoAssignmentService
    {
        Task<DoctorAutoAssignResult> SuggestDoctorAsync(string? predictedSpecialty, DateTime scheduledDate);
    }
}