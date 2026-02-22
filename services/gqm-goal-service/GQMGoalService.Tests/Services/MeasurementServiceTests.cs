using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs.Measurement;
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

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _service = new MeasurementService(_dbContext, _mapper);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistAndReturnMeasurement()
    {
        var request = new MeasurementRequest { Value = 42, TargetId = Guid.NewGuid() };
        
        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Value.Should().Be(request.Value);
        result.MeasuredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
