using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs.Target;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Exceptions;
using GQMGoalService.Infrastructure.Persistence;

namespace GQMGoalService.Application.Services;

public class TargetService : ITargetService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public TargetService(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TargetResponse>> GetAllAsync()
    {
        var targets = await _dbContext.Targets
            .Include(t => t.Measurements)
            .ToListAsync();
        return _mapper.Map<IEnumerable<TargetResponse>>(targets);
    }

    public async Task<TargetResponse> GetByIdAsync(Guid id)
    {
        var target = await _dbContext.Targets
            .Include(t => t.Measurements)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (target == null)
            throw new NotFoundException(nameof(Target), id);

        return _mapper.Map<TargetResponse>(target);
    }

    public async Task<TargetResponse> CreateAsync(TargetRequest request)
    {
        var target = _mapper.Map<Target>(request);

        _dbContext.Targets.Add(target);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<TargetResponse>(target);
    }

    public async Task<TargetResponse> UpdateAsync(Guid id, TargetRequest request)
    {
        var target = await _dbContext.Targets.FindAsync(id);
        if (target == null)
            throw new NotFoundException(nameof(Target), id);

        _mapper.Map(request, target);

        _dbContext.Targets.Update(target);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<TargetResponse>(target);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var target = await _dbContext.Targets.FindAsync(id);
        if (target == null)
            throw new NotFoundException(nameof(Target), id);

        _dbContext.Targets.Remove(target);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
