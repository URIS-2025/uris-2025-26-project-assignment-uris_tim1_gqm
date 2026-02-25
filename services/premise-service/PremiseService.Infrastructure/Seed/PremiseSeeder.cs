using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PremiseService.Domain.Entities;
using PremiseService.Domain.Enums;
using PremiseService.Infrastructure.Persistence;

namespace PremiseService.Infrastructure.Seed;

public static class PremiseSeeder
{
    public static async Task SeedAsync(PremiseDbContext context, ILogger logger)
    {
        if (await context.Premises.AnyAsync())
        {
            logger.LogInformation("Premise database already seeded. Skipping.");
            return;
        }

        logger.LogInformation("Seeding Premise database...");

        var goalId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var strategyId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        var premises = new List<Premise>
        {
            new Premise
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Description = "Team has sufficient expertise in GQM methodology",
                Type = PremiseType.Assumption,
                IsActive = true,
                GoalId = goalId,
                StrategyId = strategyId
            },
            new Premise
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Description = "Organization operates in a regulated industry requiring compliance tracking",
                Type = PremiseType.Context,
                IsActive = true,
                GoalId = goalId,
                StrategyId = strategyId
            },
            new Premise
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Description = "Initial version: Budget constraints allow for phased implementation only",
                Type = PremiseType.Assumption,
                IsActive = false,
                GoalId = goalId,
                StrategyId = strategyId
            },
            new Premise
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Description = "Updated: Budget has been approved for full implementation in Q2",
                Type = PremiseType.Assumption,
                IsActive = true,
                NewVersionOfId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                GoalId = goalId,
                StrategyId = strategyId
            }
        };

        await context.Premises.AddRangeAsync(premises);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} premises successfully.", premises.Count);
    }
}
