using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PremiseService.Application.DTOs;
using PremiseService.Application.Interfaces;
using PremiseService.Domain.Entities;
using PremiseService.Domain.Exceptions;

namespace PremiseService.Application.Services;

/// <summary>
/// Application service implementing business logic for the Premise aggregate.
/// Works directly with the database context — no repository layer.
/// </summary>
public class PremiseAppService : IPremiseService
{
    private readonly IPremiseDbContext _dbContext;
    private readonly IMapper _mapper;

    public PremiseAppService(IPremiseDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<PremiseResponse>> GetAllAsync(int page, int size)
    {
        var total = await _dbContext.Premises.CountAsync();

        var premises = await _dbContext.Premises
            .OrderBy(p => p.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .AsNoTracking()
            .ToListAsync();

        var items = _mapper.Map<IEnumerable<PremiseResponse>>(premises);
        return new PaginatedResponse<PremiseResponse>(items, page, size, total);
    }

    /// <inheritdoc />
    public async Task<PremiseResponse> GetByIdAsync(Guid id)
    {
        var premise = await _dbContext.Premises
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new PremiseNotFoundException(id);

        return _mapper.Map<PremiseResponse>(premise);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PremiseActiveResponse>> GetActiveByGoalIdAsync(Guid goalId)
    {
        var premises = await _dbContext.Premises
            .Where(p => p.GoalId == goalId && p.IsActive)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<IEnumerable<PremiseActiveResponse>>(premises);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PremiseActiveResponse>> GetActiveByStrategyIdAsync(Guid strategyId)
    {
        var premises = await _dbContext.Premises
            .Where(p => p.StrategyId == strategyId && p.IsActive)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<IEnumerable<PremiseActiveResponse>>(premises);
    }

    /// <inheritdoc />
    public async Task<PremiseResponse> CreateAsync(PremiseRequest request)
    {
        var premise = _mapper.Map<Premise>(request);
        premise.Id = Guid.NewGuid();
        premise.IsActive = true;

        await _dbContext.Premises.AddAsync(premise);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<PremiseResponse>(premise);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var premise = await _dbContext.Premises
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new PremiseNotFoundException(id);

        premise.IsActive = false;
        await _dbContext.SaveChangesAsync();
    }
}
