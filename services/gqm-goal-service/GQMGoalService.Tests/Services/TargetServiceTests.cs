using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs.Target;
using GQMGoalService.Application.Mappings;
using GQMGoalService.Application.Services;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Enums;
using GQMGoalService.Infrastructure.Persistence;

namespace GQMGoalService.Tests.Services;

public class TargetServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly TargetService _service;

    public TargetServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _dbContext = new ApplicationDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _service = new TargetService(_dbContext, _mapper, new GQMGoalService.Application.Validators.TargetRequestValidator(_dbContext));
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistAndReturnTarget()
    {
        var questionId = Guid.NewGuid();
        _dbContext.Questions.Add(new Question { Id = questionId, Text = "Test", GqmGoalId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow });
        await _dbContext.SaveChangesAsync();

        var request = new TargetRequest { Name = "Test T", Unit = Unit.Percentage, QuestionId = questionId };
        
        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be(request.Name);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
