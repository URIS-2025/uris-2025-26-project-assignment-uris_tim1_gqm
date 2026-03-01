using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Enums;

namespace AssessmentService.Infrastructure.Persistence;

public static class AssessmentSeeder
{
    public static async Task SeedAsync(AssessmentDbContext context)
    {
        if (context.GoalProbabilityAssessments.Any())
            return;

        var assessments = new List<GoalProbabilityAssessment>
        {
            new()
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                GoalId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Probability = 0.75m,
                State = AssessmentState.InProgress,
                Method = AssessmentMethod.Expert,
                Notes = "Initial expert assessment for strategic goal"
            },
            new()
            {
                Id = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                GoalId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Probability = 0.90m,
                State = AssessmentState.Completed,
                Method = AssessmentMethod.DataDriven,
                Notes = "Data-driven assessment based on historical metrics"
            },
            new()
            {
                Id = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"),
                GoalId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Probability = 0.50m,
                State = AssessmentState.Draft,
                Method = AssessmentMethod.Hybrid,
                Notes = "Preliminary hybrid assessment"
            }
        };

        await context.GoalProbabilityAssessments.AddRangeAsync(assessments);
        await context.SaveChangesAsync();
    }
}
