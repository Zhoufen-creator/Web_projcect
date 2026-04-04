using DoAnWeb.Services;
namespace DoAnWeb.Services.Interface
{
    public interface IDoctorAutoAssignmentService
    {
        Task<DoctorAutoAssignResult> SuggestDoctorAsync(string? predictedSpecialty, DateTime scheduledDate);
    }
}
