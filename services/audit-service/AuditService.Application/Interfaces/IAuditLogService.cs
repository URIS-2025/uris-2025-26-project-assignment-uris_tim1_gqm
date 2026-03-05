using AuditService.Application.DTOs;
using Shared.Contracts;

namespace AuditService.Application.Interfaces;

public interface IAuditLogService
{
    Task<AuditLogResponse?> CreateAsync(CreateAuditLogRequest request);
    Task<PaginationResponse<AuditLogResponse>> GetAllAsync(AuditLogFilter filter, PaginationRequest pagination);
    Task<PaginationResponse<AuditLogResponse>> GetByEntityAsync(string entityType, Guid entityId, PaginationRequest pagination);
    Task<PaginationResponse<AuditLogResponse>> GetByActorAsync(Guid actorId, PaginationRequest pagination);
    Task<PaginationResponse<AuditLogResponse>> GetByServiceAsync(string serviceName, PaginationRequest pagination);
}
