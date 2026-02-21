using GoalService.Domain.Entities;
using GoalService.Domain.Enums;
using GoalService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoalService.Infrastructure.Seed;

/// <summary>
/// Seeds mock data for development and testing.
/// Only runs if the database is empty.
/// Must be removed or adapted for production.
/// </summary>
public static class GoalDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GoalDbContext>();

        await context.Database.MigrateAsync();

        if (await context.Goals.AnyAsync())
            return;

        // --- Top-level Goal ---
        var topGoalId = Guid.NewGuid();
        var topGoal = new Goal
        {
            Id = topGoalId,
            Focus = "Develop the marketability",
            Object = "IP testing products",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(2),
            Magnitude = "50% coverage of customer needs for the first release",
            Constraints = "Resources, IP competence, compete with existing competitors",
            Status = GoalStatus.Active,
            BaselineProbability = 0.6m,
            DepartmentId = Guid.NewGuid() // Mock department ID (cross-service reference)
        };

        // --- Strategy for top-level goal ---
        var strategyId = Guid.NewGuid();
        var strategy = new Strategy
        {
            Id = strategyId,
            Name = "Use MoSCoW method to prioritize development efforts",
            Description = "Apply MoSCoW prioritization (Must, Should, Could, Won't) to focus development resources on the most impactful features first.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = topGoalId
        };

        // --- Child Goal (arose from the strategy) ---
        var childGoalId = Guid.NewGuid();
        var childGoal = new Goal
        {
            Id = childGoalId,
            Focus = "Develop the software product",
            Object = "IP testing business",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "100% of the MUST features and 30% of the SHOULD features",
            Constraints = "Resources, IP competence, compete with existing competitors",
            Status = GoalStatus.Active,
            BaselineProbability = 0.7m,
            DepartmentId = Guid.NewGuid() // Mock department ID
        };

        // --- GoalInfluence: child goal arose from strategy ---
        var influence = new GoalInfluence
        {
            GoalId = childGoalId,
            StrategyId = strategyId,
            InfluenceType = InfluenceType.Positive,
            Strength = 0.8m,
            Confidence = 0.75m,
            CreatedAt = DateTime.UtcNow,
            Notes = "Developing the software product directly supports marketability through feature coverage."
        };

        // --- Second Strategy (for the child goal, showing hierarchy depth) ---
        var strategy2Id = Guid.NewGuid();
        var strategy2 = new Strategy
        {
            Id = strategy2Id,
            Name = "Agile sprint-based delivery",
            Description = "Use 2-week sprints to incrementally deliver MUST and SHOULD features, enabling continuous validation with stakeholders.",
            Effectiveness = EffectivenessLevel.Medium,
            RefinementType = RefinementType.OR,
            GoalId = childGoalId
        };

        // --- Standalone Goal (no parent strategy, top of another branch) ---
        var standaloneGoalId = Guid.NewGuid();
        var standaloneGoal = new Goal
        {
            Id = standaloneGoalId,
            Focus = "Improve customer satisfaction",
            Object = "Support services",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddMonths(18),
            Magnitude = "Increase NPS score by 20 points",
            Constraints = "Budget limitations, existing support team capacity",
            Status = GoalStatus.Draft,
            BaselineProbability = 0.5m,
            DepartmentId = Guid.NewGuid()
        };

        context.Goals.AddRange(topGoal, childGoal, standaloneGoal);
        context.Strategies.AddRange(strategy, strategy2);
        context.GoalInfluences.Add(influence);

        await context.SaveChangesAsync();
    }
}
