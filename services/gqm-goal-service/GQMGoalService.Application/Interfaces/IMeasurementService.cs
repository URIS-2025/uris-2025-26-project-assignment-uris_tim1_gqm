using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Measurement;

namespace GQMGoalService.Application.Interfaces;

public interface IMeasurementService
{
    Task<PagedResult<MeasurementResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<MeasurementResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<MeasurementResponse>> GetByTargetIdAsync(Guid targetId);
    Task<MeasurementResponse> CreateAsync(MeasurementRequest request);
    Task<MeasurementResponse> UpdateAsync(Guid id, MeasurementRequest request);
    Task<bool> DeleteAsync(Guid id);
}
