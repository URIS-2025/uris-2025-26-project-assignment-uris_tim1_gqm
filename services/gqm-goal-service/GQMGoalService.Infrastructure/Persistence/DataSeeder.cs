using Microsoft.EntityFrameworkCore;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Enums;

namespace GQMGoalService.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.GqmGoals.AnyAsync())
            return; // Data already exists
            
        var goalId1 = Guid.Parse("11111111-1111-1111-1111-100000000001");
        var goalId2 = Guid.Parse("11111111-1111-1111-1111-100000000002");
        var goalId3 = Guid.Parse("11111111-1111-1111-1111-100000000003");
        var goalId4 = Guid.Parse("11111111-1111-1111-1111-100000000004");
        var goalId5 = Guid.Parse("11111111-1111-1111-1111-100000000005");
        var goalId6 = Guid.Parse("11111111-1111-1111-1111-100000000006");

        var gqmGoals = new List<GqmGoal>
        {
            new() { Id = Guid.NewGuid(), Description = "Improve customer satisfaction metrics", CreatedAt = DateTime.UtcNow.AddDays(-30), GoalId = goalId1 },
            new() { Id = Guid.NewGuid(), Description = "Reduce application response time by 20%", CreatedAt = DateTime.UtcNow.AddDays(-28), GoalId = goalId2 },
            new() { Id = Guid.NewGuid(), Description = "Increase commercial revenue margins", CreatedAt = DateTime.UtcNow.AddDays(-25), GoalId = goalId3 },
            new() { Id = Guid.NewGuid(), Description = "Enhance software quality in production", CreatedAt = DateTime.UtcNow.AddDays(-20), GoalId = goalId4 },
            new() { Id = Guid.NewGuid(), Description = "Develop employee technical skills", CreatedAt = DateTime.UtcNow.AddDays(-15), GoalId = goalId5 },
            new() { Id = Guid.NewGuid(), Description = "Reduce organizational environmental footprint", CreatedAt = DateTime.UtcNow.AddDays(-10), GoalId = goalId6 }
        };

        var questions = new List<Question>
        {
            new() { Id = Guid.NewGuid(), Text = "What is the average CSAT score this quarter?", CreatedAt = DateTime.UtcNow.AddDays(-29), GqmGoalId = gqmGoals[0].Id },
            new() { Id = Guid.NewGuid(), Text = "How many support tickets are escalated daily?", CreatedAt = DateTime.UtcNow.AddDays(-29), GqmGoalId = gqmGoals[0].Id },
            new() { Id = Guid.NewGuid(), Text = "What is the median API request latency?", CreatedAt = DateTime.UtcNow.AddDays(-27), GqmGoalId = gqmGoals[1].Id },
            new() { Id = Guid.NewGuid(), Text = "What is our Monthly Recurring Revenue (MRR)?", CreatedAt = DateTime.UtcNow.AddDays(-24), GqmGoalId = gqmGoals[2].Id },
            new() { Id = Guid.NewGuid(), Text = "How many critical defects exist in production?", CreatedAt = DateTime.UtcNow.AddDays(-19), GqmGoalId = gqmGoals[3].Id },
            new() { Id = Guid.NewGuid(), Text = "What is the average training hours per employee?", CreatedAt = DateTime.UtcNow.AddDays(-14), GqmGoalId = gqmGoals[4].Id },
            new() { Id = Guid.NewGuid(), Text = "What is the monthly energy consumption in KWh?", CreatedAt = DateTime.UtcNow.AddDays(-9), GqmGoalId = gqmGoals[5].Id }
        };

        var targets = new List<Target>
        {
            new() { Id = Guid.NewGuid(), Name = "Quarterly CSAT", Description = "Maintain CSAT above 4.5", Unit = Unit.Score, QuestionId = questions[0].Id },
            new() { Id = Guid.NewGuid(), Name = "Daily Escalations", Description = "Keep daily escalated tickets under 10", Unit = Unit.Count, QuestionId = questions[1].Id },
            new() { Id = Guid.NewGuid(), Name = "API Latency", Description = "P50 latency under 200ms", Unit = Unit.LatencyMilliseconds, QuestionId = questions[2].Id },
            new() { Id = Guid.NewGuid(), Name = "MRR Target", Description = "Reach $500,000 MRR", Unit = Unit.CurrencyPerMonth, QuestionId = questions[3].Id },
            new() { Id = Guid.NewGuid(), Name = "Prod Defects", Description = "Zero critical bugs in production", Unit = Unit.DefectCount, QuestionId = questions[4].Id },
            new() { Id = Guid.NewGuid(), Name = "Employee Training", Description = "Over 40 hours of training per employee", Unit = Unit.TrainingHours, QuestionId = questions[5].Id },
            new() { Id = Guid.NewGuid(), Name = "Energy Usage", Description = "Keep energy usage below 1000 KWh", Unit = Unit.EnergyConsumptionKWh, QuestionId = questions[6].Id }
        };

        var measurements = new List<Measurement>
        {
            new() { Id = Guid.NewGuid(), Value = 4.6m, MeasuredAt = DateTime.UtcNow.AddDays(-7), TargetId = targets[0].Id },
            new() { Id = Guid.NewGuid(), Value = 4.8m, MeasuredAt = DateTime.UtcNow.AddDays(-1), TargetId = targets[0].Id },
            new() { Id = Guid.NewGuid(), Value = 8m, MeasuredAt = DateTime.UtcNow.AddDays(-2), TargetId = targets[1].Id },
            new() { Id = Guid.NewGuid(), Value = 185m, MeasuredAt = DateTime.UtcNow.AddDays(-5), TargetId = targets[2].Id },
            new() { Id = Guid.NewGuid(), Value = 420000m, MeasuredAt = DateTime.UtcNow.AddDays(-10), TargetId = targets[3].Id },
            new() { Id = Guid.NewGuid(), Value = 450000m, MeasuredAt = DateTime.UtcNow.AddDays(-3), TargetId = targets[3].Id },
            new() { Id = Guid.NewGuid(), Value = 2m, MeasuredAt = DateTime.UtcNow.AddDays(-3), TargetId = targets[4].Id },
            new() { Id = Guid.NewGuid(), Value = 15m, MeasuredAt = DateTime.UtcNow.AddDays(-1), TargetId = targets[5].Id },
            new() { Id = Guid.NewGuid(), Value = 950m, MeasuredAt = DateTime.UtcNow.AddDays(-1), TargetId = targets[6].Id }
        };

        await context.GqmGoals.AddRangeAsync(gqmGoals);
        await context.Questions.AddRangeAsync(questions);
        await context.Targets.AddRangeAsync(targets);
        await context.Measurements.AddRangeAsync(measurements);
        
        await context.SaveChangesAsync();
    }
}
