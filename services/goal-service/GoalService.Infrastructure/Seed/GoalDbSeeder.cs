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

        // --- TechCorp: Software Engineering ---
        var techDept1 = Guid.Parse("d4e5f6a7-b8c9-0123-defa-234567890123");
        var goal1 = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Improve product release velocity",
            Object = "Core SaaS Platform",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddMonths(6),
            Magnitude = "Reduce release cycle time by 30%",
            Constraints = "Maintain current QA staffing levels",
            Status = GoalStatus.Active,
            BaselineProbability = 0.65m,
            DepartmentId = techDept1
        };
        var goal1b = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Migrate legacy monolith to microservices",
            Object = "Backend Architecture",
            ActiveFrom = DateTime.UtcNow.AddMonths(-2),
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "Extract 5 critical domains",
            Constraints = "Zero downtime during migration",
            Status = GoalStatus.Active,
            BaselineProbability = 0.40m,
            DepartmentId = techDept1
        };
        var goal1c = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Enhance API Response Time",
            Object = "Public API Gateway",
            ActiveFrom = DateTime.UtcNow.AddMonths(1),
            ActiveTo = DateTime.UtcNow.AddMonths(4),
            Magnitude = "Achieve P95 latency under 100ms",
            Constraints = "Current database hardware",
            Status = GoalStatus.Draft,
            BaselineProbability = 0.80m,
            DepartmentId = techDept1
        };

        // --- TechCorp: Quality Assurance ---
        var techDept2 = Guid.Parse("e5f6a7b8-c9d0-1234-efab-345678901234");
        var goal2 = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Enhance automated testing coverage",
            Object = "Regression Test Suite",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "Increase automated test coverage to 85%",
            Constraints = "Legacy monolithic codebase modules",
            Status = GoalStatus.Active,
            BaselineProbability = 0.50m,
            DepartmentId = techDept2
        };
        var goal2b = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Reduce escaped defects",
            Object = "Production Releases",
            ActiveFrom = DateTime.UtcNow.AddMonths(-6),
            ActiveTo = DateTime.UtcNow.AddMonths(6),
            Magnitude = "Less than 2 critical bugs per release",
            Constraints = "Fast-paced release schedule",
            Status = GoalStatus.Completed,
            BaselineProbability = 0.90m,
            DepartmentId = techDept2
        };

        // --- TechCorp: Human Resources ---
        var techDept3 = Guid.Parse("f6a7b8c9-d0e1-2345-fabc-456789012345");
        var goal3 = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Reduce employee turnover rate",
            Object = "Engineering staff",
            ActiveFrom = DateTime.UtcNow.AddMonths(-1),
            ActiveTo = DateTime.UtcNow.AddMonths(11),
            Magnitude = "Keep annual turnover below 8%",
            Constraints = "Current compensation budget",
            Status = GoalStatus.Active,
            BaselineProbability = 0.70m,
            DepartmentId = techDept3
        };
        var goal3b = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Improve Employee Onboarding",
            Object = "New Hires",
            ActiveFrom = DateTime.UtcNow.AddMonths(-12),
            ActiveTo = DateTime.UtcNow.AddMonths(-1),
            Magnitude = "Achieve 90% satisfaction score in first 30 days",
            Constraints = "Remote work environment",
            Status = GoalStatus.Completed,
            BaselineProbability = 0.85m,
            DepartmentId = techDept3
        };

        // --- GreenEnergy: Solar Division ---
        var greenDept1 = Guid.Parse("a7b8c9d0-e1f2-3456-abcd-567890123456");
        var goal4 = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Increase solar panel efficiency",
            Object = "Next-gen solar cells",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(2),
            Magnitude = "Reach 25% energy conversion efficiency",
            Constraints = "Material costs must remain under $0.50/watt",
            Status = GoalStatus.Draft,
            BaselineProbability = 0.40m,
            DepartmentId = greenDept1
        };
        var goal4b = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Reduce manufacturing waste",
            Object = "Solar Panel Production Line",
            ActiveFrom = DateTime.UtcNow.AddMonths(-3),
            ActiveTo = DateTime.UtcNow.AddMonths(9),
            Magnitude = "Decrease silicon scrap by 15%",
            Constraints = "Current manufacturing equipment",
            Status = GoalStatus.Active,
            BaselineProbability = 0.60m,
            DepartmentId = greenDept1
        };
        var goal4c = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Expand residential installations",
            Object = "B2C Sales",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "Install 5,000 new residential units",
            Constraints = "Supply chain logistics",
            Status = GoalStatus.Active,
            BaselineProbability = 0.75m,
            DepartmentId = greenDept1
        };

        // --- GreenEnergy: Wind Energy ---
        var greenDept2 = Guid.Parse("b8c9d0e1-f2a3-4567-bcde-678901234567");
        var goal5 = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Optimize wind turbine maintenance",
            Object = "Offshore wind farms",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddMonths(18),
            Magnitude = "Reduce unplanned downtime by 40%",
            Constraints = "Harsh offshore weather conditions",
            Status = GoalStatus.Active,
            BaselineProbability = 0.60m,
            DepartmentId = greenDept2
        };
        var goal5b = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Develop Predictive Maintenance Models",
            Object = "Turbine Sensor Data",
            ActiveFrom = DateTime.UtcNow.AddMonths(-5),
            ActiveTo = DateTime.UtcNow.AddMonths(4),
            Magnitude = "Predict 80% of component failures 30 days in advance",
            Constraints = "Data quality from legacy sensors",
            Status = GoalStatus.Active,
            BaselineProbability = 0.55m,
            DepartmentId = greenDept2
        };

        // --- HealthPlus: Cardiology ---
        var healthDept1 = Guid.Parse("c9d0e1f2-a3b4-5678-cdef-789012345678");
        var goal6 = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Reduce patient readmission rates",
            Object = "Heart failure patients",
            ActiveFrom = DateTime.UtcNow.AddMonths(-3),
            ActiveTo = DateTime.UtcNow.AddMonths(9),
            Magnitude = "Lower readmission rate within 30 days to under 15%",
            Constraints = "Patient compliance with post-discharge instructions",
            Status = GoalStatus.Active,
            BaselineProbability = 0.55m,
            DepartmentId = healthDept1
        };
        var goal6b = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Implement Telemonitoring Program",
            Object = "Post-op Cardiology Patients",
            ActiveFrom = DateTime.UtcNow.AddMonths(2),
            ActiveTo = DateTime.UtcNow.AddMonths(14),
            Magnitude = "Enroll 500 patients in the first year",
            Constraints = "Funding for monitoring devices",
            Status = GoalStatus.Draft,
            BaselineProbability = 0.70m,
            DepartmentId = healthDept1
        };

        // --- HealthPlus: Radiology ---
        var healthDept2 = Guid.Parse("d0e1f2a3-b4c5-6789-defa-890123456789");
        var goal7 = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Decrease report turnaround time",
            Object = "MRI and CT scan reports",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddMonths(6),
            Magnitude = "Deliver 90% of reports within 4 hours",
            Constraints = "Current radiologist staffing ratio",
            Status = GoalStatus.Active,
            BaselineProbability = 0.80m,
            DepartmentId = healthDept2
        };
        var goal7b = new Goal
        {
            Id = Guid.NewGuid(),
            Focus = "Integrate AI Triage System",
            Object = "Incoming Scans",
            ActiveFrom = DateTime.UtcNow.AddMonths(-8),
            ActiveTo = DateTime.UtcNow.AddMonths(-2),
            Magnitude = "Automatically flag 95% of critical anomalies for priority review",
            Constraints = "Integration with legacy PACS system",
            Status = GoalStatus.Completed,
            BaselineProbability = 0.85m,
            DepartmentId = healthDept2
        };

        context.Goals.AddRange(
            goal1, goal1b, goal1c,
            goal2, goal2b,
            goal3, goal3b,
            goal4, goal4b, goal4c,
            goal5, goal5b,
            goal6, goal6b,
            goal7, goal7b
        );

        // =====================================================
        // STRATEGIES
        // =====================================================

        // --- Strategies for TechCorp: Software Engineering ---

        // Goal 1: Improve product release velocity
        var strategy1a = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "CI/CD Pipeline Optimization",
            Description = "Implement automated build, test, and deployment pipelines to reduce manual intervention and accelerate releases.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = goal1.Id,
            IsActive = true
        };
        var strategy1b = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Sprint Planning Improvements",
            Description = "Adopt capacity-based sprint planning with buffer allocation for unplanned work to improve predictability.",
            Effectiveness = EffectivenessLevel.Medium,
            RefinementType = RefinementType.AND,
            GoalId = goal1.Id,
            IsActive = true
        };

        // Goal 1b: Migrate legacy monolith to microservices
        var strategy1c = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Domain-Driven Design Implementation",
            Description = "Apply DDD principles to identify bounded contexts and define clear service boundaries.",
            Effectiveness = EffectivenessLevel.VeryHigh,
            RefinementType = RefinementType.AND,
            GoalId = goal1b.Id,
            IsActive = true
        };
        var strategy1d = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Strangler Fig Pattern",
            Description = "Gradually replace monolith functionality by routing traffic to new microservices while maintaining backward compatibility.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.OR,
            GoalId = goal1b.Id,
            IsActive = true
        };

        // Goal 1c: Enhance API Response Time
        var strategy1e = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Distributed Caching Strategy",
            Description = "Implement Redis-based caching for frequently accessed data to reduce database load and improve response times.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = goal1c.Id,
            IsActive = true
        };
        var strategy1f = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Query Optimization Program",
            Description = "Analyze and optimize slow database queries using execution plans and indexing strategies.",
            Effectiveness = EffectivenessLevel.Medium,
            RefinementType = RefinementType.AND,
            GoalId = goal1c.Id,
            IsActive = true
        };

        // --- Strategies for TechCorp: Quality Assurance ---

        // Goal 2: Enhance automated testing coverage
        var strategy2a = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Test Pyramid Implementation",
            Description = "Restructure test suite following the test pyramid: 70% unit, 20% integration, 10% E2E tests.",
            Effectiveness = EffectivenessLevel.VeryHigh,
            RefinementType = RefinementType.AND,
            GoalId = goal2.Id,
            IsActive = true
        };
        var strategy2b = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Contract Testing Adoption",
            Description = "Implement Pact-based contract testing for microservice APIs to catch integration issues early.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = goal2.Id,
            IsActive = true
        };

        // Goal 2b: Reduce escaped defects
        var strategy2c = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Pre-release Quality Gates",
            Description = "Enforce mandatory code coverage thresholds and static analysis checks before any release.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = goal2b.Id,
            IsActive = false // Completed goal, strategy no longer active
        };
        var strategy2d = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Shift-Left Testing Initiative",
            Description = "Move testing earlier in the development cycle with TDD practices and developer-owned testing.",
            Effectiveness = EffectivenessLevel.VeryHigh,
            RefinementType = RefinementType.AND,
            GoalId = goal2b.Id,
            IsActive = false
        };

        // --- Strategies for TechCorp: Human Resources ---

        // Goal 3: Reduce employee turnover rate
        var strategy3a = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Career Growth Framework",
            Description = "Define clear career ladders with skill matrices and promotion criteria for engineering roles.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = goal3.Id,
            IsActive = true
        };
        var strategy3b = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Competitive Compensation Review",
            Description = "Conduct quarterly market analysis and adjust compensation to remain within top 25% of industry benchmarks.",
            Effectiveness = EffectivenessLevel.VeryHigh,
            RefinementType = RefinementType.AND,
            GoalId = goal3.Id,
            IsActive = true
        };

        // Goal 3b: Improve Employee Onboarding
        var strategy3c = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Buddy System Program",
            Description = "Pair each new hire with an experienced team member for their first 90 days.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.OR,
            GoalId = goal3b.Id,
            IsActive = false
        };
        var strategy3d = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Structured 30-60-90 Plan",
            Description = "Create role-specific onboarding plans with clear milestones and check-ins at 30, 60, and 90 days.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = goal3b.Id,
            IsActive = false
        };

        // --- Strategies for GreenEnergy: Solar Division ---

        // Goal 4: Increase solar panel efficiency
        var strategy4a = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Perovskite Cell Research",
            Description = "Invest in R&D for perovskite-silicon tandem cells to achieve higher conversion efficiency.",
            Effectiveness = EffectivenessLevel.VeryHigh,
            RefinementType = RefinementType.OR,
            GoalId = goal4.Id,
            IsActive = true
        };
        var strategy4b = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Anti-Reflective Coating Enhancement",
            Description = "Develop advanced nano-textured anti-reflective coatings to maximize light absorption.",
            Effectiveness = EffectivenessLevel.Medium,
            RefinementType = RefinementType.AND,
            GoalId = goal4.Id,
            IsActive = true
        };

        // Goal 4b: Reduce manufacturing waste
        var strategy4c = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Lean Manufacturing Implementation",
            Description = "Apply lean principles to eliminate waste in silicon wafer cutting and cell assembly processes.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = goal4b.Id,
            IsActive = true
        };

        // Goal 4c: Expand residential installations
        var strategy4d = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Partner Network Expansion",
            Description = "Recruit and certify 200 additional residential installers across target markets.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = goal4c.Id,
            IsActive = true
        };
        var strategy4e = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Digital Marketing Campaign",
            Description = "Launch targeted social media and SEO campaigns to generate qualified residential leads.",
            Effectiveness = EffectivenessLevel.Medium,
            RefinementType = RefinementType.OR,
            GoalId = goal4c.Id,
            IsActive = true
        };

        // --- Strategies for GreenEnergy: Wind Energy ---

        // Goal 5: Optimize wind turbine maintenance
        var strategy5a = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Predictive Analytics Platform",
            Description = "Deploy ML-based predictive analytics using SCADA data to forecast component failures.",
            Effectiveness = EffectivenessLevel.VeryHigh,
            RefinementType = RefinementType.AND,
            GoalId = goal5.Id,
            IsActive = true
        };
        var strategy5b = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Drone Inspection Program",
            Description = "Use autonomous drones for blade and tower inspections to reduce technician exposure and inspection time.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.OR,
            GoalId = goal5.Id,
            IsActive = true
        };

        // Goal 5b: Develop Predictive Maintenance Models
        var strategy5c = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "ML Model Development",
            Description = "Train gradient boosting and neural network models on historical failure data for component prediction.",
            Effectiveness = EffectivenessLevel.VeryHigh,
            RefinementType = RefinementType.AND,
            GoalId = goal5b.Id,
            IsActive = true
        };
        var strategy5d = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "IoT Sensor Upgrades",
            Description = "Replace legacy sensors with high-frequency IoT sensors to improve data quality for ML models.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = goal5b.Id,
            IsActive = true
        };

        // --- Strategies for HealthPlus: Cardiology ---

        // Goal 6: Reduce patient readmission rates
        var strategy6a = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Remote Patient Monitoring",
            Description = "Provide heart failure patients with connected monitoring devices for daily vital sign tracking.",
            Effectiveness = EffectivenessLevel.VeryHigh,
            RefinementType = RefinementType.AND,
            GoalId = goal6.Id,
            IsActive = true
        };
        var strategy6b = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Discharge Education Program",
            Description = "Implement structured discharge education with medication reconciliation and follow-up scheduling.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = goal6.Id,
            IsActive = true
        };

        // Goal 6b: Implement Telemonitoring Program
        var strategy6c = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Wearable Device Integration",
            Description = "Partner with wearable manufacturers to integrate patient data directly into EHR systems.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = goal6b.Id,
            IsActive = true
        };

        // --- Strategies for HealthPlus: Radiology ---

        // Goal 7: Decrease report turnaround time
        var strategy7a = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Workflow Automation System",
            Description = "Implement RIS-integrated workflow automation to auto-assign cases based on radiologist availability and expertise.",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = goal7.Id,
            IsActive = true
        };
        var strategy7b = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Voice Recognition Dictation",
            Description = "Deploy AI-powered voice recognition for real-time report dictation and transcription.",
            Effectiveness = EffectivenessLevel.Medium,
            RefinementType = RefinementType.OR,
            GoalId = goal7.Id,
            IsActive = true
        };

        // Goal 7b: Integrate AI Triage System
        var strategy7c = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Deep Learning Anomaly Detection",
            Description = "Deploy CNN-based models trained on annotated imaging data to automatically detect and flag critical findings.",
            Effectiveness = EffectivenessLevel.VeryHigh,
            RefinementType = RefinementType.AND,
            GoalId = goal7b.Id,
            IsActive = false // Completed goal
        };

        context.Strategies.AddRange(
            strategy1a, strategy1b, strategy1c, strategy1d, strategy1e, strategy1f,
            strategy2a, strategy2b, strategy2c, strategy2d,
            strategy3a, strategy3b, strategy3c, strategy3d,
            strategy4a, strategy4b, strategy4c, strategy4d, strategy4e,
            strategy5a, strategy5b, strategy5c, strategy5d,
            strategy6a, strategy6b, strategy6c,
            strategy7a, strategy7b, strategy7c
        );

        // =====================================================
        // GOAL INFLUENCES (Hierarchies)
        // =====================================================
        // These create parent-child relationships between goals via strategies.
        // Pattern: Parent Goal → Strategy → Child Goal (via GoalInfluence)

        var goalInfluences = new List<GoalInfluence>
        {
            // TechCorp Software Engineering:
            // goal1 (Release velocity) → strategy1a (CI/CD) → goal1b (Microservices migration)
            // Rationale: CI/CD pipeline work enables microservices deployment
            new GoalInfluence
            {
                GoalId = goal1b.Id,
                StrategyId = strategy1a.Id,
                InfluenceType = InfluenceType.Positive,
                Strength = 0.75m,
                Confidence = 0.85m,
                CreatedAt = DateTime.UtcNow,
                Notes = "CI/CD pipeline optimization directly enables safe microservice deployments"
            },

            // goal1 (Release velocity) → strategy1b (Sprint Planning) → goal1c (API latency)
            // Rationale: Better sprint planning allows focus on performance improvements
            new GoalInfluence
            {
                GoalId = goal1c.Id,
                StrategyId = strategy1b.Id,
                InfluenceType = InfluenceType.Positive,
                Strength = 0.50m,
                Confidence = 0.70m,
                CreatedAt = DateTime.UtcNow,
                Notes = "Improved sprint planning creates capacity for API optimization work"
            },

            // GreenEnergy Solar Division:
            // goal4 (Panel efficiency) → strategy4a (Perovskite research) → goal4b (Reduce waste)
            // Rationale: New cell technology requires optimized manufacturing processes
            new GoalInfluence
            {
                GoalId = goal4b.Id,
                StrategyId = strategy4a.Id,
                InfluenceType = InfluenceType.Positive,
                Strength = 0.60m,
                Confidence = 0.65m,
                CreatedAt = DateTime.UtcNow,
                Notes = "Perovskite cell manufacturing improvements reduce silicon waste"
            },

            // GreenEnergy Wind Energy:
            // goal5 (Maintenance optimization) → strategy5a (Predictive Analytics) → goal5b (Predictive Models)
            // Rationale: Analytics platform is foundation for predictive maintenance models
            new GoalInfluence
            {
                GoalId = goal5b.Id,
                StrategyId = strategy5a.Id,
                InfluenceType = InfluenceType.Positive,
                Strength = 0.90m,
                Confidence = 0.95m,
                CreatedAt = DateTime.UtcNow,
                Notes = "Predictive analytics platform directly supports ML model development"
            },

            // HealthPlus Cardiology:
            // goal6 (Reduce readmissions) → strategy6a (Remote Monitoring) → goal6b (Telemonitoring Program)
            // Rationale: Remote monitoring success leads to expanded telemonitoring program
            new GoalInfluence
            {
                GoalId = goal6b.Id,
                StrategyId = strategy6a.Id,
                InfluenceType = InfluenceType.Positive,
                Strength = 0.85m,
                Confidence = 0.90m,
                CreatedAt = DateTime.UtcNow,
                Notes = "Remote monitoring infrastructure enables broader telemonitoring enrollment"
            },

            // HealthPlus Radiology:
            // goal7 (Report turnaround) → strategy7a (Workflow Automation) → goal7b (AI Triage)
            // Rationale: Automated workflow integrates with AI triage for prioritization
            new GoalInfluence
            {
                GoalId = goal7b.Id,
                StrategyId = strategy7a.Id,
                InfluenceType = InfluenceType.Positive,
                Strength = 0.70m,
                Confidence = 0.80m,
                CreatedAt = DateTime.UtcNow,
                Notes = "Workflow automation system integrates AI triage for case prioritization"
            }
        };

        context.GoalInfluences.AddRange(goalInfluences);

        await context.SaveChangesAsync();
    }
}
