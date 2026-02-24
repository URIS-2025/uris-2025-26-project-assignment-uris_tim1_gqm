using Microsoft.EntityFrameworkCore;
using GQMGoalService.Domain.Entities;

namespace GQMGoalService.Application.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext, allowing the Application layer to access
/// entity sets without depending on the Infrastructure layer directly.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<GqmGoal> GqmGoals { get; }
    DbSet<Question> Questions { get; }
    DbSet<Target> Targets { get; }
    DbSet<Measurement> Measurements { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
