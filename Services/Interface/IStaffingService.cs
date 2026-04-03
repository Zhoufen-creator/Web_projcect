namespace DoAnWeb.Services.Interface
{
    public interface IStaffingService
    {
        /// <summary>
        /// Tính số lượng bác sĩ cần trực cho chuyên khoa vào tuần tới
        /// </summary>
        Task<int> CalculateRequiredDoctorsAsync(int specialtyId);

        /// <summary>
        /// Tính số lượng bác sĩ cần trực cho tất cả chuyên khoa
        /// </summary>
        Task<Dictionary<int, int>> CalculateRequiredDoctorsForAllSpecialtiesAsync();
    }
}
