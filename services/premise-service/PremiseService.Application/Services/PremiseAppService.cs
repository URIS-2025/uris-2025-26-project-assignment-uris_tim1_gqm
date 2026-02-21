using AutoMapper;
using PremiseService.Application.DTOs;
using PremiseService.Application.Interfaces;
using PremiseService.Domain.Entities;
using PremiseService.Domain.Exceptions;

namespace PremiseService.Application.Services;

/// <summary>
/// Application service implementing business logic for the Premise aggregate.
/// Handles CRUD operations with built-in versioning support.
/// </summary>
public class PremiseAppService : IPremiseService
{
    private readonly IPremiseRepository _repository;
    private readonly IMapper _mapper;

    public PremiseAppService(IPremiseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PremiseResponse>> GetAllActiveAsync()
    {
        var premises = await _repository.GetAllActiveAsync();
        return _mapper.Map<IEnumerable<PremiseResponse>>(premises);
    }

    /// <inheritdoc />
    public async Task<PremiseResponse> GetByIdAsync(Guid id)
    {
        var premise = await _repository.GetByIdAsync(id)
            ?? throw new PremiseNotFoundException(id);

        return _mapper.Map<PremiseResponse>(premise);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PremiseResponse>> GetByGoalIdAsync(Guid goalId)
    {
        var premises = await _repository.GetByGoalIdAsync(goalId);
        return _mapper.Map<IEnumerable<PremiseResponse>>(premises);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PremiseResponse>> GetByStrategyIdAsync(Guid strategyId)
    {
        var premises = await _repository.GetByStrategyIdAsync(strategyId);
        return _mapper.Map<IEnumerable<PremiseResponse>>(premises);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PremiseResponse>> GetVersionHistoryAsync(Guid premiseId)
    {
        var premise = await _repository.GetByIdAsync(premiseId)
            ?? throw new PremiseNotFoundException(premiseId);

        var history = await _repository.GetVersionHistoryAsync(premiseId);
        return _mapper.Map<IEnumerable<PremiseResponse>>(history);
    }

    /// <inheritdoc />
    public async Task<PremiseResponse> CreateAsync(PremiseRequest request)
    {
        var premise = _mapper.Map<Premise>(request);
        premise.Id = Guid.NewGuid();
        premise.IsActive = true;

        var created = await _repository.CreateAsync(premise);
        await _repository.SaveChangesAsync();

        return _mapper.Map<PremiseResponse>(created);
    }

    /// <inheritdoc />
    public async Task<PremiseResponse> UpdateAsync(Guid id, PremiseUpdateRequest request)
    {
        var existingPremise = await _repository.GetByIdAsync(id)
            ?? throw new PremiseNotFoundException(id);

        // Deactivate the old version
        existingPremise.IsActive = false;
        await _repository.UpdateAsync(existingPremise);

        // Create a new version with updated description, inheriting Type, GoalId, and StrategyId
        var newVersion = new Premise
        {
            Id = Guid.NewGuid(),
            Description = request.Description,
            Type = existingPremise.Type,
            IsActive = true,
            NewVersionOfId = existingPremise.Id,
            GoalId = existingPremise.GoalId,
            StrategyId = existingPremise.StrategyId
        };

        var created = await _repository.CreateAsync(newVersion);
        await _repository.SaveChangesAsync();

        return _mapper.Map<PremiseResponse>(created);
    }

    /// <inheritdoc />
    public async Task DeactivateAsync(Guid id)
    {
        var premise = await _repository.GetByIdAsync(id)
            ?? throw new PremiseNotFoundException(id);

        premise.IsActive = false;
        await _repository.UpdateAsync(premise);
        await _repository.SaveChangesAsync();
    }
}
