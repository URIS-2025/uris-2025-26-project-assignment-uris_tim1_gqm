using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Question;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Exceptions;
using FluentValidation;

namespace GQMGoalService.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IValidator<QuestionRequest> _validator;

    public QuestionService(IApplicationDbContext dbContext, IMapper mapper, IValidator<QuestionRequest> validator)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<PagedResult<QuestionResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        var totalCount = await _dbContext.Questions.CountAsync();
        var questions = await _dbContext.Questions
            .Include(q => q.Targets)
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        var dtos = _mapper.Map<IEnumerable<QuestionResponse>>(questions);
        return new PagedResult<QuestionResponse>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<QuestionResponse> GetByIdAsync(Guid id)
    {
        var question = await _dbContext.Questions
            .Include(q => q.Targets)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id);
            
        if (question == null)
            throw new NotFoundException(nameof(Question), id);

        return _mapper.Map<QuestionResponse>(question);
    }

    public async Task<IEnumerable<QuestionResponse>> GetByGqmGoalIdAsync(Guid gqmGoalId)
    {
        var questions = await _dbContext.Questions
            .Include(q => q.Targets)
            .Where(q => q.GqmGoalId == gqmGoalId)
            .AsNoTracking()
            .ToListAsync();
            
        return _mapper.Map<IEnumerable<QuestionResponse>>(questions);
    }

    public async Task<QuestionResponse> CreateAsync(QuestionRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var question = _mapper.Map<Question>(request);
        question.CreatedAt = DateTime.UtcNow;

        _dbContext.Questions.Add(question);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<QuestionResponse>(question);
    }

    public async Task<QuestionResponse> UpdateAsync(Guid id, QuestionRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

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

        bool hasTargets = await _dbContext.Targets.AnyAsync(t => t.QuestionId == id);
        if (hasTargets)
            throw new InvalidOperationException("Cannot delete Question because it has associated targets.");

        _dbContext.Questions.Remove(question);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
