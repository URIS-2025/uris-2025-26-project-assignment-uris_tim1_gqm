using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Target;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Exceptions;
using FluentValidation;

namespace GQMGoalService.Application.Services;

public class TargetService : ITargetService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IValidator<TargetRequest> _validator;

    public TargetService(IApplicationDbContext dbContext, IMapper mapper, IValidator<TargetRequest> validator)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<PagedResult<TargetResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbContext.Targets.CountAsync(cancellationToken);
        var targets = await _dbContext.Targets
            .Include(t => t.Measurements)
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<IEnumerable<TargetResponse>>(targets);
        return new PagedResult<TargetResponse>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<TargetResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var target = await _dbContext.Targets
            .Include(t => t.Measurements)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
            
        if (target == null)
            throw new NotFoundException(nameof(Target), id);

        return _mapper.Map<TargetResponse>(target);
    }

    public async Task<IEnumerable<TargetResponse>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        var targets = await _dbContext.Targets
            .Include(t => t.Measurements)
            .Where(t => t.QuestionId == questionId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
            
        return _mapper.Map<IEnumerable<TargetResponse>>(targets);
    }

    public async Task<TargetResponse> CreateAsync(TargetRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var target = _mapper.Map<Target>(request);

        _dbContext.Targets.Add(target);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TargetResponse>(target);
    }

    public async Task<TargetResponse> UpdateAsync(Guid id, TargetRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var target = await _dbContext.Targets.FindAsync(new object[] { id }, cancellationToken);
        if (target == null)
            throw new NotFoundException(nameof(Target), id);

        _mapper.Map(request, target);

        _dbContext.Targets.Update(target);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TargetResponse>(target);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var target = await _dbContext.Targets.FindAsync(new object[] { id }, cancellationToken);
        if (target == null)
            throw new NotFoundException(nameof(Target), id);

        bool hasMeasurements = await _dbContext.Measurements.AnyAsync(m => m.TargetId == id, cancellationToken);
        if (hasMeasurements)
            throw new ConflictException("Cannot delete Target because it has associated measurements.");

        _dbContext.Targets.Remove(target);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
