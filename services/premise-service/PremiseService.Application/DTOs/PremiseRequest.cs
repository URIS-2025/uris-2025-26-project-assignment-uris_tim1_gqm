using PremiseService.Domain.Enums;

namespace PremiseService.Application.DTOs;

public class PremiseRequest
{
    public string Description { get; set; } = string.Empty;
    public PremiseType Type { get; set; }
    public Guid GoalId { get; set; }
    public Guid StrategyId { get; set; }
}
