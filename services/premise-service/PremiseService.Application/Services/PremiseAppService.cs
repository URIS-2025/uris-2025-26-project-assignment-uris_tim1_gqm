using AutoMapper;
using PremiseService.Application.DTOs;
using PremiseService.Application.Interfaces;
using PremiseService.Domain.Entities;
using PremiseService.Domain.Exceptions;

namespace PremiseService.Application.Services;

public class PremiseAppService : IPremiseService
{
    private readonly IPremiseRepository _repository;
    private readonly IMapper _mapper;

    public PremiseAppService(IPremiseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PremiseResponse>> GetAllActiveAsync()
    {
        var premises = await _repository.GetAllActiveAsync();
        return _mapper.Map<IEnumerable<PremiseResponse>>(premises);
    }

    public async Task<PremiseResponse> GetByIdAsync(Guid id)
    {
        var premise = await _repository.GetByIdAsync(id)
            ?? throw new PremiseNotFoundException(id);

        return _mapper.Map<PremiseResponse>(premise);
    }

    public async Task<IEnumerable<PremiseResponse>> GetByGoalIdAsync(Guid goalId)
    {
        var premises = await _repository.GetByGoalIdAsync(goalId);
        return _mapper.Map<IEnumerable<PremiseResponse>>(premises);
    }

    public async Task<IEnumerable<PremiseResponse>> GetByStrategyIdAsync(Guid strategyId)
    {
        var premises = await _repository.GetByStrategyIdAsync(strategyId);
        return _mapper.Map<IEnumerable<PremiseResponse>>(premises);
    }

    public async Task<IEnumerable<PremiseResponse>> GetVersionHistoryAsync(Guid premiseId)
    {
        var premise = await _repository.GetByIdAsync(premiseId)
            ?? throw new PremiseNotFoundException(premiseId);

        var history = await _repository.GetVersionHistoryAsync(premiseId);
        return _mapper.Map<IEnumerable<PremiseResponse>>(history);
    }

    public async Task<PremiseResponse> CreateAsync(PremiseRequest request)
    {
        var premise = _mapper.Map<Premise>(request);
        premise.Id = Guid.NewGuid();
        premise.IsActive = true;

        var created = await _repository.CreateAsync(premise);
        await _repository.SaveChangesAsync();

        return _mapper.Map<PremiseResponse>(created);
    }

    public async Task<PremiseResponse> UpdateAsync(Guid id, PremiseUpdateRequest request)
    {
        var existingPremise = await _repository.GetByIdAsync(id)
            ?? throw new PremiseNotFoundException(id);

        // Deactivate the old version
        existingPremise.IsActive = false;
        await _repository.UpdateAsync(existingPremise);

        // Create a new version with updated description
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

    public async Task DeactivateAsync(Guid id)
    {
        var premise = await _repository.GetByIdAsync(id)
            ?? throw new PremiseNotFoundException(id);

        premise.IsActive = false;
        await _repository.UpdateAsync(premise);
        await _repository.SaveChangesAsync();
    }
}
