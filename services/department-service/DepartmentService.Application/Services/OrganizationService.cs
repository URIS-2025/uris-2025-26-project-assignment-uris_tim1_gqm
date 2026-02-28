using AutoMapper;
using DepartmentService.Application.DTOs;
using Shared.Contracts;
using DepartmentService.Application.Interfaces;
using DepartmentService.Domain.Entities;
using DepartmentService.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DepartmentService.Application.Services;

public class OrganizationService : IOrganizationService
{
    private readonly DbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<OrganizationRequest> _validator;

    public OrganizationService(DbContext context, IMapper mapper, IValidator<OrganizationRequest> validator)
    {
        _context = context;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<PaginationResponse<OrganizationResponse>> GetAllAsync(int page, int size)
    {
        var query = _context.Set<Organization>().AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(o => o.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PaginationResponse<OrganizationResponse>
        {
            Items = _mapper.Map<List<OrganizationResponse>>(items),
            Total = totalCount,
            PageNumber = page,
            PageSize = size
        };
    }

    public async Task<OrganizationResponse> GetByIdAsync(Guid id)
    {
        var organization = await _context.Set<Organization>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization is null)
            throw new NotFoundException(nameof(Organization), id);

        return _mapper.Map<OrganizationResponse>(organization);
    }

    public async Task<OrganizationResponse> CreateAsync(OrganizationRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var organization = _mapper.Map<Organization>(request);
        organization.Id = Guid.NewGuid();
        organization.CreatedAt = DateTime.UtcNow;
        organization.UpdatedAt = DateTime.UtcNow;

        _context.Set<Organization>().Add(organization);
        await _context.SaveChangesAsync();

        return _mapper.Map<OrganizationResponse>(organization);
    }

    public async Task<OrganizationResponse> UpdateAsync(Guid id, OrganizationRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var organization = await _context.Set<Organization>()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization is null)
            throw new NotFoundException(nameof(Organization), id);

        organization.Name = request.Name;
        organization.Description = request.Description;
        organization.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<OrganizationResponse>(organization);
    }

    public async Task DeleteAsync(Guid id)
    {
        var organization = await _context.Set<Organization>()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization is null)
            throw new NotFoundException(nameof(Organization), id);

        _context.Set<Organization>().Remove(organization);
        await _context.SaveChangesAsync();
    }
}
