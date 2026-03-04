using UserService.Application.DTOs;
using Shared.Contracts;

namespace UserService.Application.Interfaces;

public interface IUserService
{
    Task<PaginationResponse<UserResponse>> GetAllAsync(int page, int size);
    Task<UserResponse> GetByIdAsync(Guid id);
    Task<UserResponse> GetByEmailAsync(string email);
    Task<UserResponse> CreateAsync(UserRequest request);
    Task<UserResponse> UpdateProfileAsync(Guid id, UpdateProfileRequest request);
    Task<UserResponse> ToggleIsActiveAsync(Guid id);
    Task DeleteAsync(Guid id);
}
