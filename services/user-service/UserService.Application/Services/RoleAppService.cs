using AutoMapper;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace UserService.Application.Services;

public class RoleAppService : IRoleService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<RoleRequest> _validator;

    public RoleAppService(IApplicationDbContext context, IMapper mapper, IValidator<RoleRequest> validator)
    {
        _context = context;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<List<RoleResponse>> GetAllAsync()
    {
        var roles = await _context.Set<Role>()
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync();

        return _mapper.Map<List<RoleResponse>>(roles);
    }

    public async Task<RoleResponse> GetByIdAsync(Guid id)
    {
        var role = await _context.Set<Role>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role is null)
            throw new NotFoundException(nameof(Role), id);

        return _mapper.Map<RoleResponse>(role);
    }

    public async Task<RoleResponse> CreateAsync(RoleRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var nameExists = await _context.Set<Role>()
            .AnyAsync(r => r.Name == request.Name);

        if (nameExists)
            throw new BadRequestException($"A role with name '{request.Name}' already exists.");

        var role = _mapper.Map<Role>(request);
        role.Id = Guid.NewGuid();
        role.CreatedAt = DateTime.UtcNow;
        role.UpdatedAt = DateTime.UtcNow;

        _context.Set<Role>().Add(role);
        await _context.SaveChangesAsync();

        return _mapper.Map<RoleResponse>(role);
    }

    public async Task<RoleResponse> UpdateAsync(Guid id, RoleRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var role = await _context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role is null)
            throw new NotFoundException(nameof(Role), id);

        var nameExists = await _context.Set<Role>()
            .AnyAsync(r => r.Name == request.Name && r.Id != id);

        if (nameExists)
            throw new BadRequestException($"A role with name '{request.Name}' already exists.");

        role.Name = request.Name;
        role.Description = request.Description;
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<RoleResponse>(role);
    }

    public async Task DeleteAsync(Guid id)
    {
        var role = await _context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role is null)
            throw new NotFoundException(nameof(Role), id);

        if (Roles.IsSystemRole(role.Name))
            throw new BadRequestException("System roles cannot be deleted.");

        _context.Set<Role>().Remove(role);
        await _context.SaveChangesAsync();
    }
}
