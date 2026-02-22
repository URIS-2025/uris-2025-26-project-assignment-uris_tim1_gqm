using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs.Question;
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

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _service = new QuestionService(_dbContext, _mapper);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistAndReturnQuestion()
    {
        var request = new QuestionRequest { Text = "Test Q", GqmGoalId = Guid.NewGuid() };
        
        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Text.Should().Be(request.Text);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
