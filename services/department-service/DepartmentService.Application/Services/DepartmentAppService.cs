using AutoMapper;
using DepartmentService.Application.DTOs;
using Shared.Contracts;
using DepartmentService.Application.Interfaces;
using DepartmentService.Domain.Entities;
using DepartmentService.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DepartmentService.Application.Services;

public class DepartmentAppService : IDepartmentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<DepartmentRequest> _validator;

    public DepartmentAppService(IApplicationDbContext context, IMapper mapper, IValidator<DepartmentRequest> validator)
    {
        _context = context;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<PaginationResponse<DepartmentResponse>> GetAllAsync(int page, int size)
    {
        var query = _context.Set<Department>().AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(d => d.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PaginationResponse<DepartmentResponse>
        {
            Items = _mapper.Map<List<DepartmentResponse>>(items),
            Total = totalCount,
            PageNumber = page,
            PageSize = size
        };
    }

    public async Task<PaginationResponse<DepartmentResponse>> GetByOrganizationIdAsync(Guid organizationId, int page, int size)
    {
        var organizationExists = await _context.Set<Organization>()
            .AnyAsync(o => o.Id == organizationId);

        if (!organizationExists)
            throw new NotFoundException(nameof(Organization), organizationId);

        var query = _context.Set<Department>()
            .AsNoTracking()
            .Where(d => d.OrganizationId == organizationId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(d => d.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PaginationResponse<DepartmentResponse>
        {
            Items = _mapper.Map<List<DepartmentResponse>>(items),
            Total = totalCount,
            PageNumber = page,
            PageSize = size
        };
    }

    public async Task<DepartmentResponse> GetByIdAsync(Guid id)
    {
        var department = await _context.Set<Department>()
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department is null)
            throw new NotFoundException(nameof(Department), id);

        return _mapper.Map<DepartmentResponse>(department);
    }

    public async Task<DepartmentResponse> CreateAsync(DepartmentRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var organizationExists = await _context.Set<Organization>()
            .AnyAsync(o => o.Id == request.OrganizationId);

        if (!organizationExists)
            throw new NotFoundException(nameof(Organization), request.OrganizationId);

        var department = _mapper.Map<Department>(request);
        department.Id = Guid.NewGuid();
        department.CreatedAt = DateTime.UtcNow;
        department.UpdatedAt = DateTime.UtcNow;

        _context.Set<Department>().Add(department);
        await _context.SaveChangesAsync();

        return _mapper.Map<DepartmentResponse>(department);
    }

    public async Task<DepartmentResponse> UpdateAsync(Guid id, DepartmentRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var department = await _context.Set<Department>()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department is null)
            throw new NotFoundException(nameof(Department), id);

        var organizationExists = await _context.Set<Organization>()
            .AnyAsync(o => o.Id == request.OrganizationId);

        if (!organizationExists)
            throw new NotFoundException(nameof(Organization), request.OrganizationId);

        department.Name = request.Name;
        department.Description = request.Description;
        department.OrganizationId = request.OrganizationId;
        department.ManagerId = request.ManagerId;
        department.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<DepartmentResponse>(department);
    }

    public async Task DeleteAsync(Guid id)
    {
        var department = await _context.Set<Department>()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department is null)
            throw new NotFoundException(nameof(Department), id);

        _context.Set<Department>().Remove(department);
        await _context.SaveChangesAsync();
    }
}
