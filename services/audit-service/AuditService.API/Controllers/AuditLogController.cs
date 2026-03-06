using AuditService.Application.DTOs;
using AuditService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace AuditService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogController> _logger;

    public AuditLogController(IAuditLogService auditLogService, ILogger<AuditLogController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Creates an audit log entry. Always returns 2xx — never propagates errors to callers.
    /// </summary>
    [HttpPost("log")]
    [ProducesResponseType(typeof(AuditLogResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Log([FromBody] CreateAuditLogRequest request)
    {
        try
        {
            var result = await _auditLogService.CreateAsync(request);
            if (result is null)
                return Ok(new { message = "Audit log could not be persisted but request was accepted." });

            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in AuditLog POST /audit/log");
            return Ok(new { message = "Audit log request acknowledged." });
        }
    }

    /// <summary>
    /// Returns paginated audit logs with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] AuditLogFilter filter,
        [FromQuery] PaginationRequest pagination)
    {
        var result = await _auditLogService.GetAllAsync(filter, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Returns audit logs for a specific entity.
    /// </summary>
    [HttpGet("{entityType}/{entityId:guid}")]
    [ProducesResponseType(typeof(PaginationResponse<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntity(
        string entityType,
        Guid entityId,
        [FromQuery] PaginationRequest pagination)
    {
        var result = await _auditLogService.GetByEntityAsync(entityType, entityId, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Returns audit logs performed by a specific actor.
    /// </summary>
    [HttpGet("actor/{actorId:guid}")]
    [ProducesResponseType(typeof(PaginationResponse<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByActor(
        Guid actorId,
        [FromQuery] PaginationRequest pagination)
    {
        var result = await _auditLogService.GetByActorAsync(actorId, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Returns audit logs from a specific service.
    /// </summary>
    [HttpGet("service/{serviceName}")]
    [ProducesResponseType(typeof(PaginationResponse<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByService(
        string serviceName,
        [FromQuery] PaginationRequest pagination)
    {
        var result = await _auditLogService.GetByServiceAsync(serviceName, pagination);
        return Ok(result);
    }
}
