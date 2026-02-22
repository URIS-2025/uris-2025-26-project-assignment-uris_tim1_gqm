using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Measurement;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Exceptions;
using GQMGoalService.Infrastructure.Persistence;
using FluentValidation;

namespace GQMGoalService.Application.Services;

public class MeasurementService : IMeasurementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IValidator<MeasurementRequest> _validator;

    public MeasurementService(ApplicationDbContext dbContext, IMapper mapper, IValidator<MeasurementRequest> validator)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<PagedResult<MeasurementResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        var totalCount = await _dbContext.Measurements.CountAsync();
        var measurements = await _dbContext.Measurements
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        var dtos = _mapper.Map<IEnumerable<MeasurementResponse>>(measurements);
        return new PagedResult<MeasurementResponse>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<MeasurementResponse> GetByIdAsync(Guid id)
    {
        var measurement = await _dbContext.Measurements.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (measurement == null)
            throw new NotFoundException(nameof(Measurement), id);

        return _mapper.Map<MeasurementResponse>(measurement);
    }

    public async Task<IEnumerable<MeasurementResponse>> GetByTargetIdAsync(Guid targetId)
    {
        var measurements = await _dbContext.Measurements
            .Where(m => m.TargetId == targetId)
            .AsNoTracking()
            .ToListAsync();
            
        return _mapper.Map<IEnumerable<MeasurementResponse>>(measurements);
    }

    public async Task<MeasurementResponse> CreateAsync(MeasurementRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var measurement = _mapper.Map<Measurement>(request);
        if (!request.MeasuredAt.HasValue)
        {
            measurement.MeasuredAt = DateTime.UtcNow;
        }

        _dbContext.Measurements.Add(measurement);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<MeasurementResponse>(measurement);
    }

    public async Task<MeasurementResponse> UpdateAsync(Guid id, MeasurementRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var measurement = await _dbContext.Measurements.FindAsync(id);
        if (measurement == null)
            throw new NotFoundException(nameof(Measurement), id);

        _mapper.Map(request, measurement);
        if (!request.MeasuredAt.HasValue)
        {
            measurement.MeasuredAt = DateTime.UtcNow;
        }

        _dbContext.Measurements.Update(measurement);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<MeasurementResponse>(measurement);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var measurement = await _dbContext.Measurements.FindAsync(id);
        if (measurement == null)
            throw new NotFoundException(nameof(Measurement), id);

        _dbContext.Measurements.Remove(measurement);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
