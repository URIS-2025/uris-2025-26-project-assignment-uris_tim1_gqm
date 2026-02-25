using AssessmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Infrastructure.Persistence;

public interface IAssessmentDbContext
{
    DbSet<GoalProbabilityAssessment> GoalProbabilityAssessments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
