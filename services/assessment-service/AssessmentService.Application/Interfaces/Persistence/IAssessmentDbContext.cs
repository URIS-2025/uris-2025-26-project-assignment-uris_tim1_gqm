using AssessmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Application.Interfaces;

public interface IAssessmentDbContext
{
    DbSet<GoalProbabilityAssessment> GoalProbabilityAssessments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
