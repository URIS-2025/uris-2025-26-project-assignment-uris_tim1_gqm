using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs.Question;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Application.Mappings;
using GQMGoalService.Application.Services;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Infrastructure.Persistence;

namespace GQMGoalService.Tests.Services;

public class QuestionServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly QuestionService _service;

    public QuestionServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _dbContext = new ApplicationDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<QuestionMappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _service = new QuestionService((IApplicationDbContext)_dbContext, _mapper, new GQMGoalService.Application.Validators.QuestionRequestValidator(_dbContext));
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistAndReturnQuestion()
    {
        var goalId = Guid.NewGuid();
        _dbContext.GqmGoals.Add(new GqmGoal { Id = goalId, Description = "Test", GoalId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow });
        await _dbContext.SaveChangesAsync();

        var request = new QuestionRequest { Text = "Test Q", GqmGoalId = goalId };
        
        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Text.Should().Be(request.Text);
    }

    [Fact]
    public async Task GetByGqmGoalIdAsync_ShouldThrowNotFoundException_WhenGqmGoalDoesNotExist()
    {
        var nonExistentId = Guid.NewGuid();

        var act = () => _service.GetByGqmGoalIdAsync(nonExistentId);

        await act.Should().ThrowAsync<GQMGoalService.Domain.Exceptions.NotFoundException>();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
