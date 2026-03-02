using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs.Target;
using GQMGoalService.Application.Validators;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Enums;
using GQMGoalService.Infrastructure.Persistence;

namespace GQMGoalService.Tests.Validators;

public class TargetRequestValidatorTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly TargetRequestValidator _validator;

    public TargetRequestValidatorTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _dbContext = new ApplicationDbContext(options);
        _validator = new TargetRequestValidator(_dbContext);
    }

    [Fact]
    public async Task Validate_ValidRequest_ShouldNotHaveErrors()
    {
        var qId = Guid.NewGuid();
        _dbContext.Questions.Add(new Question { Id = qId, Text = "Test" });
        await _dbContext.SaveChangesAsync();

        var request = new TargetRequest { Name = "Valid", Unit = Unit.Percentage, QuestionId = qId };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }
}
