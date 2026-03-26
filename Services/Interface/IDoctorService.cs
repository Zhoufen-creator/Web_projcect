using DoAnWeb.Models;

namespace DoAnWeb.Services;
public interface IDoctorService
{
    public Task<IEnumerable<MedicalExamination>> GetAllMedicalExaminationAsync();
}