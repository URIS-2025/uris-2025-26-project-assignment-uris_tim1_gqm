using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Measurement;

namespace GQMGoalService.Application.Interfaces;

public interface IMeasurementService
{
    Task<PagedResult<MeasurementResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<MeasurementResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MeasurementResponse>> GetByTargetIdAsync(Guid targetId, CancellationToken cancellationToken = default);
    Task<MeasurementResponse> CreateAsync(MeasurementRequest request, CancellationToken cancellationToken = default);
    Task<MeasurementResponse> UpdateAsync(Guid id, MeasurementRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
