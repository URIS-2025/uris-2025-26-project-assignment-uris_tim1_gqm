namespace GoalService.Application.DTOs;

public record ActivationReadinessResponse(bool CanActivate, IReadOnlyList<string> Blockers);
