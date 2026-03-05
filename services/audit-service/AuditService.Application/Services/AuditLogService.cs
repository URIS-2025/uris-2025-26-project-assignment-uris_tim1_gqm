using AuditService.Application.DTOs;
using AuditService.Application.Interfaces;
using AuditService.Domain.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using System.Text.Json;

namespace AuditService.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(IApplicationDbContext context, IMapper mapper, ILogger<AuditLogService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuditLogResponse?> CreateAsync(CreateAuditLogRequest request)
    {
        try
        {
            string? metadataJson = null;
            if (request.Metadata is not null)
            {
                metadataJson = JsonSerializer.Serialize(request.Metadata);
            }

            var auditLog = new AuditLog(
                request.ActorId,
                request.ActorRole,
                request.Service,
                request.Action,
                request.EntityType,
                request.EntityId,
                metadataJson
            );

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return _mapper.Map<AuditLogResponse>(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for action {Action} on {EntityType}/{EntityId}",
                request.Action, request.EntityType, request.EntityId);
            return null;
        }
    }

    public async Task<PaginationResponse<AuditLogResponse>> GetAllAsync(AuditLogFilter filter, PaginationRequest pagination)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (filter.Service is not null)
            query = query.Where(x => x.Service == filter.Service);
        if (filter.Action is not null)
            query = query.Where(x => x.Action == filter.Action);
        if (filter.EntityType is not null)
            query = query.Where(x => x.EntityType == filter.EntityType);
        if (filter.ActorId.HasValue)
            query = query.Where(x => x.ActorId == filter.ActorId.Value);
        if (filter.EntityId.HasValue)
            query = query.Where(x => x.EntityId == filter.EntityId.Value);
        if (filter.From.HasValue)
            query = query.Where(x => x.Timestamp >= filter.From.Value);
        if (filter.To.HasValue)
            query = query.Where(x => x.Timestamp <= filter.To.Value);

        return await ToPaginatedResponseAsync(query, pagination);
    }

    public async Task<PaginationResponse<AuditLogResponse>> GetByEntityAsync(string entityType, Guid entityId, PaginationRequest pagination)
    {
        var query = _context.AuditLogs
            .Where(x => x.EntityType == entityType && x.EntityId == entityId);

        return await ToPaginatedResponseAsync(query, pagination);
    }

    public async Task<PaginationResponse<AuditLogResponse>> GetByActorAsync(Guid actorId, PaginationRequest pagination)
    {
        var query = _context.AuditLogs
            .Where(x => x.ActorId == actorId);

        return await ToPaginatedResponseAsync(query, pagination);
    }

    public async Task<PaginationResponse<AuditLogResponse>> GetByServiceAsync(string serviceName, PaginationRequest pagination)
    {
        var query = _context.AuditLogs
            .Where(x => x.Service == serviceName);

        return await ToPaginatedResponseAsync(query, pagination);
    }

    private async Task<PaginationResponse<AuditLogResponse>> ToPaginatedResponseAsync(
        IQueryable<AuditLog> query, PaginationRequest pagination)
    {
        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PaginationResponse<AuditLogResponse>
        {
            Items = _mapper.Map<IEnumerable<AuditLogResponse>>(items),
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            Total = total
        };
    }
}
