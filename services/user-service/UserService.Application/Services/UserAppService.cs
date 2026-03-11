using AutoMapper;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;

namespace UserService.Application.Services;

public class UserAppService : IUserService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<UserRequest> _validator;
    private readonly IValidator<UpdateProfileRequest> _profileValidator;

    public UserAppService(
        IApplicationDbContext context,
        IMapper mapper,
        IValidator<UserRequest> validator,
        IValidator<UpdateProfileRequest> profileValidator)
    {
        _context = context;
        _mapper = mapper;
        _validator = validator;
        _profileValidator = profileValidator;
    }

    public async Task<PaginationResponse<UserResponse>> GetAllAsync(int page, int size, Guid currentUserId, bool isSystemAdmin, Guid? currentOrgId)
    {
        var query = _context.Set<User>()
            .AsNoTracking()
            .Include(u => u.UserOrganizationRoles)
                .ThenInclude(uor => uor.Role)
            .AsQueryable();

        // Organization filtering logic
        if (currentOrgId.HasValue)
        {
            // Both SystemAdmins and normal users will be filtered by the selected organization context
            query = query.Where(u => u.OrganizationId == currentOrgId.Value);
        }
        else if (!isSystemAdmin)
        {
            // If not system admin and no org context, return empty for safety
            return new PaginationResponse<UserResponse>
            {
                Items = new List<UserResponse>(),
                Total = 0,
                PageNumber = page,
                PageSize = size
            };
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var items = users.Select(u => 
        {
            var response = _mapper.Map<UserResponse>(u);
            
            // Map the roles for the current organization context, or all roles if system admin without org context
            response.Roles = u.UserOrganizationRoles
                .Select(uor => uor.Role?.Name ?? "Unknown Role")
                .Distinct()
                .ToList();
            return response;
        }).ToList();

        return new PaginationResponse<UserResponse>
        {
            Items = items,
            Total = totalCount,
            PageNumber = page,
            PageSize = size
        };
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var user = await _context.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            throw new NotFoundException(nameof(User), id);

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> GetByEmailAsync(string email)
    {
        var user = await _context.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
            throw new NotFoundException($"User with email '{email}' was not found.");

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> CreateAsync(UserRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var emailExists = await _context.Set<User>()
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
            throw new BadRequestException($"A user with email '{request.Email}' already exists.");

        var user = _mapper.Map<User>(request);
        user.Id = Guid.NewGuid();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.IsActive = true;
        user.OrganizationId = request.OrganizationId;
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Set<User>().Add(user);
        await _context.SaveChangesAsync();

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> UpdateProfileAsync(Guid id, UpdateProfileRequest request)
    {
        var validationResult = await _profileValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            throw new NotFoundException(nameof(User), id);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> ToggleIsActiveAsync(Guid id)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            throw new NotFoundException(nameof(User), id);

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<UserResponse>(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            throw new NotFoundException(nameof(User), id);

        _context.Set<User>().Remove(user);
        await _context.SaveChangesAsync();
    }
}
