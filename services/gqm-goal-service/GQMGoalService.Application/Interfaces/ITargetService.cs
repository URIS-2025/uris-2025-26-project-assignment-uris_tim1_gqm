using GQMGoalService.Application.DTOs.Target;

namespace GQMGoalService.Application.Interfaces;

public interface ITargetService
{
    Task<IEnumerable<TargetResponse>> GetAllAsync();
    Task<TargetResponse> GetByIdAsync(Guid id);
    Task<TargetResponse> CreateAsync(TargetRequest request);
    Task<TargetResponse> UpdateAsync(Guid id, TargetRequest request);
    Task<bool> DeleteAsync(Guid id);
}
