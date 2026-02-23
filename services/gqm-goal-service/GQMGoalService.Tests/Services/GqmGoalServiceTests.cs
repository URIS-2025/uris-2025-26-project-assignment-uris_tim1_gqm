using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs.GqmGoal;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Application.Mappings;
using GQMGoalService.Application.Services;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Infrastructure.Persistence;

namespace GQMGoalService.Tests.Services;

public class GqmGoalServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly GqmGoalService _service;

    public GqmGoalServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _dbContext = new ApplicationDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _service = new GqmGoalService((IApplicationDbContext)_dbContext, _mapper, new GQMGoalService.Application.Validators.GqmGoalRequestValidator());
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistAndReturnGoal()
    {
        var request = new GqmGoalRequest { Description = "Test Goal", GoalId = Guid.NewGuid() };
        
        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Description.Should().Be(request.Description);
        
        var dbGoal = await _dbContext.GqmGoals.FindAsync(result.Id);
        dbGoal.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnGoal()
    {
        var goal = new GqmGoal { Id = Guid.NewGuid(), Description = "Exist", GoalId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        _dbContext.GqmGoals.Add(goal);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetByIdAsync(goal.Id);
        
        result.Should().NotBeNull();
        result.Id.Should().Be(goal.Id);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
