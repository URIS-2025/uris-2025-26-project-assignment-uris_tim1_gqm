using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs.Question;
using GQMGoalService.Application.Validators;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Infrastructure.Persistence;

namespace GQMGoalService.Tests.Validators;

public class QuestionRequestValidatorTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly QuestionRequestValidator _validator;

    public QuestionRequestValidatorTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _dbContext = new ApplicationDbContext(options);
        _validator = new QuestionRequestValidator(_dbContext);
    }

    [Fact]
    public async Task Validate_ValidRequest_ShouldNotHaveErrors()
    {
        var goalId = Guid.NewGuid();
        _dbContext.GqmGoals.Add(new GqmGoal { Id = goalId, Description = "Test" });
        await _dbContext.SaveChangesAsync();

        var request = new QuestionRequest { Text = "Valid Text", GqmGoalId = goalId };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_NonExistentGoalId_ShouldHaveError()
    {
        var request = new QuestionRequest { Text = "Valid Text", GqmGoalId = Guid.NewGuid() };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "GqmGoalId");
    }
}
