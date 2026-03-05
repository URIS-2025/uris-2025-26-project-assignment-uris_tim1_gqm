using AuditService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditService.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
