using GQMGoalService.Application.DTOs.Measurement;

namespace GQMGoalService.Application.Interfaces;

public interface IMeasurementService
{
    Task<IEnumerable<MeasurementResponse>> GetAllAsync();
    Task<MeasurementResponse> GetByIdAsync(Guid id);
    Task<MeasurementResponse> CreateAsync(MeasurementRequest request);
    Task<MeasurementResponse> UpdateAsync(Guid id, MeasurementRequest request);
    Task<bool> DeleteAsync(Guid id);
}
