using PremiseService.Domain.Enums;

namespace PremiseService.Domain.Entities;

public class Premise
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public PremiseType Type { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? NewVersionOfId { get; set; }
    public Guid GoalId { get; set; }
    public Guid StrategyId { get; set; }

    // Self-referencing navigation for version history
    public Premise? NewVersionOf { get; set; }
    public Premise? NewerVersion { get; set; }
}
