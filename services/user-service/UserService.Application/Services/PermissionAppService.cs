using AutoMapper;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace UserService.Application.Services;

public class PermissionAppService : IPermissionService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<PermissionRequest> _validator;

    public PermissionAppService(IApplicationDbContext context, IMapper mapper, IValidator<PermissionRequest> validator)
    {
        _context = context;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<List<PermissionResponse>> GetAllAsync()
    {
        var permissions = await _context.Set<Permission>()
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();

        return _mapper.Map<List<PermissionResponse>>(permissions);
    }

    public async Task<PermissionResponse> GetByIdAsync(Guid id)
    {
        var permission = await _context.Set<Permission>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (permission is null)
            throw new NotFoundException(nameof(Permission), id);

        return _mapper.Map<PermissionResponse>(permission);
    }

    public async Task<PermissionResponse> CreateAsync(PermissionRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var nameExists = await _context.Set<Permission>()
            .AnyAsync(p => p.Name == request.Name);

        if (nameExists)
            throw new BadRequestException($"A permission with name '{request.Name}' already exists.");

        var permission = _mapper.Map<Permission>(request);
        permission.Id = Guid.NewGuid();
        permission.CreatedAt = DateTime.UtcNow;
        permission.UpdatedAt = DateTime.UtcNow;

        _context.Set<Permission>().Add(permission);
        await _context.SaveChangesAsync();

        return _mapper.Map<PermissionResponse>(permission);
    }

    public async Task<PermissionResponse> UpdateAsync(Guid id, PermissionRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var permission = await _context.Set<Permission>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (permission is null)
            throw new NotFoundException(nameof(Permission), id);

        var nameExists = await _context.Set<Permission>()
            .AnyAsync(p => p.Name == request.Name && p.Id != id);

        if (nameExists)
            throw new BadRequestException($"A permission with name '{request.Name}' already exists.");

        permission.Name = request.Name;
        permission.Description = request.Description;
        permission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<PermissionResponse>(permission);
    }

    public async Task DeleteAsync(Guid id)
    {
        var permission = await _context.Set<Permission>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (permission is null)
            throw new NotFoundException(nameof(Permission), id);

        if (Permissions.IsSystemPermission(permission.Name))
            throw new BadRequestException("System permissions cannot be deleted.");

        _context.Set<Permission>().Remove(permission);
        await _context.SaveChangesAsync();
    }
}
