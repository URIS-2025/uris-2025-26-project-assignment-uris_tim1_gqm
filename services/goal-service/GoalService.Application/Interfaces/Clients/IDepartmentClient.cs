using GoalService.Application.DTOs.External;
using Shared.Contracts;

namespace GoalService.Application.Interfaces.Clients;

public interface IDepartmentClient
{
    Task<IEnumerable<Guid>> GetMyDepartmentIdsAsync();
    Task<DepartmentDto?> GetDepartmentAsync(Guid departmentId);
}
