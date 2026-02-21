using PremiseService.Domain.Enums;

namespace PremiseService.Application.DTOs;

public class PremiseResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public PremiseType Type { get; set; }
    public bool IsActive { get; set; }
    public Guid? NewVersionOfId { get; set; }
    public Guid GoalId { get; set; }
    public Guid StrategyId { get; set; }
}
