using PremiseService.Application.DTOs.External;

namespace PremiseService.Application.Interfaces.Clients;

public interface IStrategyClient
{
    Task<StrategyDto?> GetStrategyByIdAsync(Guid strategyId);
}
