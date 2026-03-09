using AuditService.Application.DTOs;
using AuditService.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Messages;
using System;
using System.Threading.Tasks;

namespace AuditService.Infrastructure.Consumers;

public class AuditLogCreatedConsumer : IConsumer<IAuditLogCreated>
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogCreatedConsumer> _logger;

    public AuditLogCreatedConsumer(IAuditLogService auditLogService, ILogger<AuditLogCreatedConsumer> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IAuditLogCreated> context)
    {
        try
        {
            var msg = context.Message;
            
            var request = new CreateAuditLogRequest(
                ActorId: msg.ActorId,
                ActorRole: msg.ActorRole,
                Service: msg.Service,
                Action: msg.Action,
                EntityType: msg.EntityType,
                EntityId: msg.EntityId,
                Metadata: msg.Metadata
            );

            await _auditLogService.CreateAsync(request);
            
            _logger.LogInformation("Successfully consumed AuditLogCreated event for Action {Action} on {EntityType}/{EntityId}", 
                msg.Action, msg.EntityType, msg.EntityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to consume AuditLogCreated event. Swallowing error to avoid message loop or poison queue.");
            // We swallow here because audit logs are not critical enough to fail workflows or cause retries
        }
    }
}
