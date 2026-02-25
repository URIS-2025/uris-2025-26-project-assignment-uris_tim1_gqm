using GoalService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GoalService.Application.Interfaces.Persistence;

public interface IGoalDbContext
{
    DbSet<Goal> Goals { get; }
    DbSet<Strategy> Strategies { get; }
    DbSet<GoalInfluence> GoalInfluences { get; }
    
    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
