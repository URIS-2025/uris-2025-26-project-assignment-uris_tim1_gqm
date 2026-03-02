using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs.Measurement;
using GQMGoalService.Application.Validators;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Infrastructure.Persistence;

namespace GQMGoalService.Tests.Validators;

public class MeasurementRequestValidatorTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly MeasurementRequestValidator _validator;

    public MeasurementRequestValidatorTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _dbContext = new ApplicationDbContext(options);
        _validator = new MeasurementRequestValidator(_dbContext);
    }

    [Fact]
    public async Task Validate_ValidRequest_ShouldNotHaveErrors()
    {
        var tId = Guid.NewGuid();
        _dbContext.Targets.Add(new Target { Id = tId, Name = "Test" });
        await _dbContext.SaveChangesAsync();

        var request = new MeasurementRequest { Value = 10, TargetId = tId };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }
}
