using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs.Question;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Exceptions;
using GQMGoalService.Infrastructure.Persistence;

namespace GQMGoalService.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public QuestionService(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<IEnumerable<QuestionResponse>> GetAllAsync()
    {
        var questions = await _dbContext.Questions
            .Include(q => q.Targets)
            .ToListAsync();
        return _mapper.Map<IEnumerable<QuestionResponse>>(questions);
    }

    public async Task<QuestionResponse> GetByIdAsync(Guid id)
    {
        var question = await _dbContext.Questions
            .Include(q => q.Targets)
            .FirstOrDefaultAsync(q => q.Id == id);
            
        if (question == null)
            throw new NotFoundException(nameof(Question), id);

        return _mapper.Map<QuestionResponse>(question);
    }

    public async Task<QuestionResponse> CreateAsync(QuestionRequest request)
    {
        var question = _mapper.Map<Question>(request);
        question.CreatedAt = DateTime.UtcNow;

        _dbContext.Questions.Add(question);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<QuestionResponse>(question);
    }

    public async Task<QuestionResponse> UpdateAsync(Guid id, QuestionRequest request)
    {
        var question = await _dbContext.Questions.FindAsync(id);
        if (question == null)
            throw new NotFoundException(nameof(Question), id);

        _mapper.Map(request, question);

        _dbContext.Questions.Update(question);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<QuestionResponse>(question);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var question = await _dbContext.Questions.FindAsync(id);
        if (question == null)
            throw new NotFoundException(nameof(Question), id);

        _dbContext.Questions.Remove(question);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
