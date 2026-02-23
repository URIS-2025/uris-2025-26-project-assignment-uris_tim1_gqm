using Microsoft.EntityFrameworkCore;
using GQMGoalService.Domain.Entities;

namespace GQMGoalService.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<GqmGoal> GqmGoals { get; }
    DbSet<Question> Questions { get; }
    DbSet<Target> Targets { get; }
    DbSet<Measurement> Measurements { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
