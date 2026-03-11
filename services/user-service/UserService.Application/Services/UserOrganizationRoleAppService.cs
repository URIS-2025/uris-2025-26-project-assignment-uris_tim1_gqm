using AutoMapper;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace UserService.Application.Services;

public class UserOrganizationRoleAppService : IUserOrganizationRoleService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<AssignRoleRequest> _validator;

    public UserOrganizationRoleAppService(
        IApplicationDbContext context,
        IMapper mapper,
        IValidator<AssignRoleRequest> validator)
    {
        _context = context;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<UserOrganizationRoleResponse> AssignRoleAsync(AssignRoleRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var userExists = await _context.Set<User>()
            .AnyAsync(u => u.Id == request.UserId);

        if (!userExists)
            throw new NotFoundException(nameof(User), request.UserId);

        var role = await _context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId);

        if (role is null)
            throw new NotFoundException(nameof(Role), request.RoleId);

        var alreadyAssigned = await _context.Set<UserOrganizationRole>()
            .AnyAsync(uor =>
                uor.UserId == request.UserId &&
                uor.RoleId == request.RoleId);

        if (alreadyAssigned)
            throw new BadRequestException("This role is already assigned to the user.");

        var userOrganizationRole = new UserOrganizationRole
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            RoleId = request.RoleId
        };

        _context.Set<UserOrganizationRole>().Add(userOrganizationRole);
        await _context.SaveChangesAsync();

        return new UserOrganizationRoleResponse
        {
            UserId = userOrganizationRole.UserId,
            RoleId = userOrganizationRole.RoleId,
            RoleName = role.Name
        };
    }

    public async Task RemoveRoleAsync(Guid userId, Guid roleId)
    {
        var assignment = await _context.Set<UserOrganizationRole>()
            .FirstOrDefaultAsync(uor =>
                uor.UserId == userId &&
                uor.RoleId == roleId);

        if (assignment is null)
            throw new NotFoundException("Role assignment not found.");

        _context.Set<UserOrganizationRole>().Remove(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task<List<UserOrganizationRoleResponse>> GetByUserIdAsync(Guid userId)
    {
        var userExists = await _context.Set<User>()
            .AnyAsync(u => u.Id == userId);

        if (!userExists)
            throw new NotFoundException(nameof(User), userId);

        var assignments = await _context.Set<UserOrganizationRole>()
            .AsNoTracking()
            .Include(uor => uor.Role)
            .Where(uor => uor.UserId == userId)
            .ToListAsync();

        return _mapper.Map<List<UserOrganizationRoleResponse>>(assignments);
    }

    public async Task<List<UserOrganizationRoleResponse>> GetByUserAndOrganizationAsync(Guid userId, Guid organizationId)
    {
        var userExists = await _context.Set<User>()
            .AnyAsync(u => u.Id == userId && u.OrganizationId == organizationId);

        if (!userExists)
            throw new NotFoundException(nameof(User), userId);

        var assignments = await _context.Set<UserOrganizationRole>()
            .AsNoTracking()
            .Include(uor => uor.Role)
            .Where(uor => uor.UserId == userId)
            .ToListAsync();

        return _mapper.Map<List<UserOrganizationRoleResponse>>(assignments);
    }
}
