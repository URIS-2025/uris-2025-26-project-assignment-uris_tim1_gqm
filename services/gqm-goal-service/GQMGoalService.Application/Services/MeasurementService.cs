using Shared.Contracts;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.Measurement;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Exceptions;
using FluentValidation;

namespace GQMGoalService.Application.Services;

public class MeasurementService : IMeasurementService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IValidator<MeasurementRequest> _validator;

    public MeasurementService(IApplicationDbContext dbContext, IMapper mapper, IValidator<MeasurementRequest> validator)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<PaginationResponse<MeasurementResponse>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbContext.Measurements.CountAsync(cancellationToken);
        var measurements = await _dbContext.Measurements
            .AsNoTracking()
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<IEnumerable<MeasurementResponse>>(measurements);
        return new PaginationResponse<MeasurementResponse>
        {
            Items = dtos,
            Total = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<MeasurementResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var measurement = await _dbContext.Measurements.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (measurement == null)
            throw new NotFoundException(nameof(Measurement), id);

        return _mapper.Map<MeasurementResponse>(measurement);
    }

    public async Task<IEnumerable<MeasurementResponse>> GetByTargetIdAsync(Guid targetId, CancellationToken cancellationToken = default)
    {
        var targetExists = await _dbContext.Targets.AnyAsync(t => t.Id == targetId, cancellationToken);
        if (!targetExists)
            throw new NotFoundException(nameof(Target), targetId);

        var measurements = await _dbContext.Measurements
            .Where(m => m.TargetId == targetId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
            
        return _mapper.Map<IEnumerable<MeasurementResponse>>(measurements);
    }

    public async Task<MeasurementResponse> CreateAsync(MeasurementRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var measurement = _mapper.Map<Measurement>(request);

        // Default to current UTC time if the client did not supply a measurement timestamp
        if (!request.MeasuredAt.HasValue)
        {
            measurement.MeasuredAt = DateTime.UtcNow;
        }

        _dbContext.Measurements.Add(measurement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MeasurementResponse>(measurement);
    }

    public async Task<MeasurementResponse> UpdateAsync(Guid id, MeasurementRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var measurement = await _dbContext.Measurements.FindAsync(new object[] { id }, cancellationToken);
        if (measurement == null)
            throw new NotFoundException(nameof(Measurement), id);

        _mapper.Map(request, measurement);

        // Preserve existing timestamp unless the client explicitly provides a new one
        if (!request.MeasuredAt.HasValue)
        {
            measurement.MeasuredAt = DateTime.UtcNow;
        }

        _dbContext.Measurements.Update(measurement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MeasurementResponse>(measurement);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var measurement = await _dbContext.Measurements.FindAsync(new object[] { id }, cancellationToken);
        if (measurement == null)
            throw new NotFoundException(nameof(Measurement), id);

        _dbContext.Measurements.Remove(measurement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
