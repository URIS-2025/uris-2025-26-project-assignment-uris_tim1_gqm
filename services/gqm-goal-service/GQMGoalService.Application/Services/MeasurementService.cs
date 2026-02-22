using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs.Measurement;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Exceptions;
using GQMGoalService.Infrastructure.Persistence;

namespace GQMGoalService.Application.Services;

public class MeasurementService : IMeasurementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public MeasurementService(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MeasurementResponse>> GetAllAsync()
    {
        var measurements = await _dbContext.Measurements.ToListAsync();
        return _mapper.Map<IEnumerable<MeasurementResponse>>(measurements);
    }

    public async Task<MeasurementResponse> GetByIdAsync(Guid id)
    {
        var measurement = await _dbContext.Measurements.FindAsync(id);
        if (measurement == null)
            throw new NotFoundException(nameof(Measurement), id);

        return _mapper.Map<MeasurementResponse>(measurement);
    }

    public async Task<MeasurementResponse> CreateAsync(MeasurementRequest request)
    {
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
