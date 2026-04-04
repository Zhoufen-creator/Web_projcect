namespace DoAnWeb.Services.Interface
{
    public interface ISeasonalStaffingDetectionService
    {
        Task<IReadOnlyList<SeasonalStaffingAlert>> DetectAsync(CancellationToken cancellationToken = default);
        Task<int> DetectAndNotifyEmployeesAsync(CancellationToken cancellationToken = default);
    }
}
