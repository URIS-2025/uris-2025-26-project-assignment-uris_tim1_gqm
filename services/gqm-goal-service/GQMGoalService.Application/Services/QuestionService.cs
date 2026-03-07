using Shared.Contracts;
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

    public async Task<PaginationResponse<QuestionResponse>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Questions
            .Include(q => q.Targets)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.OrderBy))
        {
            query = request.OrderBy.ToLower() switch
            {
                "text" => query.OrderBy(q => q.Text),
                "createdat" => query.OrderBy(q => q.CreatedAt),
                _ => query.OrderBy(q => q.Id)
            };
        }
        else
        {
            query = query.OrderBy(q => q.Id);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var questions = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<IEnumerable<QuestionResponse>>(questions);
        return new PaginationResponse<QuestionResponse>
        {
            Items = dtos,
            Total = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<QuestionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var question = await _dbContext.Questions
            .Include(q => q.Targets)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
            
        if (question == null)
            throw new NotFoundException(nameof(Question), id);

        return _mapper.Map<QuestionResponse>(question);
    }

    public async Task<IEnumerable<QuestionResponse>> GetByGqmGoalIdAsync(Guid gqmGoalId, CancellationToken cancellationToken = default)
    {
        var gqmGoalExists = await _dbContext.GqmGoals.AnyAsync(g => g.Id == gqmGoalId, cancellationToken);
        if (!gqmGoalExists)
            throw new NotFoundException(nameof(GqmGoal), gqmGoalId);

        var questions = await _dbContext.Questions
            .Include(q => q.Targets)
            .Where(q => q.GqmGoalId == gqmGoalId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
            
        return _mapper.Map<IEnumerable<QuestionResponse>>(questions);
    }

    public async Task<QuestionResponse> CreateAsync(QuestionRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        
        var gqmGoalExists = await _dbContext.GqmGoals.AnyAsync(g => g.Id == request.GqmGoalId, cancellationToken);
        if (!gqmGoalExists)
            throw new NotFoundException(nameof(GqmGoal), request.GqmGoalId);

        var question = _mapper.Map<Question>(request);
        question.CreatedAt = DateTime.UtcNow;

        _dbContext.Questions.Add(question);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<QuestionResponse>(question);
    }

    public async Task<QuestionResponse> UpdateAsync(Guid id, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var gqmGoalExists = await _dbContext.GqmGoals.AnyAsync(g => g.Id == request.GqmGoalId, cancellationToken);
        if (!gqmGoalExists)
            throw new NotFoundException(nameof(GqmGoal), request.GqmGoalId);

        var question = await _dbContext.Questions.FindAsync(new object[] { id }, cancellationToken);
        if (question == null)
            throw new NotFoundException(nameof(Question), id);

        _mapper.Map(request, question);

        _dbContext.Questions.Update(question);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<QuestionResponse>(question);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var question = await _dbContext.Questions.FindAsync(new object[] { id }, cancellationToken);
        if (question == null)
            throw new NotFoundException(nameof(Question), id);

        bool hasTargets = await _dbContext.Targets.AnyAsync(t => t.QuestionId == id, cancellationToken);
        if (hasTargets)
            throw new ConflictException("Cannot delete Question because it has associated targets.");

        _dbContext.Questions.Remove(question);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
