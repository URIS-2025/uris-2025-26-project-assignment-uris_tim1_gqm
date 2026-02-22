using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Target;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Exceptions;
using GQMGoalService.Infrastructure.Persistence;
using FluentValidation;

namespace GQMGoalService.Application.Services;

public class TargetService : ITargetService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IValidator<TargetRequest> _validator;

    public TargetService(ApplicationDbContext dbContext, IMapper mapper, IValidator<TargetRequest> validator)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<PagedResult<TargetResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        var totalCount = await _dbContext.Targets.CountAsync();
        var targets = await _dbContext.Targets
            .Include(t => t.Measurements)
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        var dtos = _mapper.Map<IEnumerable<TargetResponse>>(targets);
        return new PagedResult<TargetResponse>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<TargetResponse> GetByIdAsync(Guid id)
    {
        var target = await _dbContext.Targets
            .Include(t => t.Measurements)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (target == null)
            throw new NotFoundException(nameof(Target), id);

        return _mapper.Map<TargetResponse>(target);
    }

    public async Task<IEnumerable<TargetResponse>> GetByQuestionIdAsync(Guid questionId)
    {
        var targets = await _dbContext.Targets
            .Include(t => t.Measurements)
            .Where(t => t.QuestionId == questionId)
            .AsNoTracking()
            .ToListAsync();
            
        return _mapper.Map<IEnumerable<TargetResponse>>(targets);
    }

    public async Task<TargetResponse> CreateAsync(TargetRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var target = _mapper.Map<Target>(request);

        _dbContext.Targets.Add(target);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<TargetResponse>(target);
    }

    public async Task<TargetResponse> UpdateAsync(Guid id, TargetRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

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

        bool hasMeasurements = await _dbContext.Measurements.AnyAsync(m => m.TargetId == id);
        if (hasMeasurements)
            throw new InvalidOperationException("Cannot delete Target because it has associated measurements.");

        _dbContext.Targets.Remove(target);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
