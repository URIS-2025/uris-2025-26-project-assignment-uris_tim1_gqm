using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs.Measurement;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Application.Mappings;
using GQMGoalService.Application.Services;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Infrastructure.Persistence;

namespace GQMGoalService.Tests.Services;

public class MeasurementServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly MeasurementService _service;

    public MeasurementServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _dbContext = new ApplicationDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MeasurementMappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _service = new MeasurementService((IApplicationDbContext)_dbContext, _mapper, new GQMGoalService.Application.Validators.MeasurementRequestValidator(_dbContext));
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistAndReturnMeasurement()
    {
        var targetId = Guid.NewGuid();
        _dbContext.Targets.Add(new Target { Id = targetId, Name = "Test", QuestionId = Guid.NewGuid() });
        await _dbContext.SaveChangesAsync();

        var request = new MeasurementRequest { Value = 42, TargetId = targetId };
        
        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Value.Should().Be(request.Value);
        result.MeasuredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetByTargetIdAsync_ShouldThrowNotFoundException_WhenTargetDoesNotExist()
    {
        var nonExistentId = Guid.NewGuid();

        var act = () => _service.GetByTargetIdAsync(nonExistentId);

        await act.Should().ThrowAsync<GQMGoalService.Domain.Exceptions.NotFoundException>();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
