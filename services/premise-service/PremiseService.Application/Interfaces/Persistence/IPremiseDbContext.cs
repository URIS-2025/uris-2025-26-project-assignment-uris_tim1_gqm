using Microsoft.EntityFrameworkCore;
using PremiseService.Domain.Entities;

namespace PremiseService.Application.Interfaces;

/// <summary>
/// Database context interface for the Premise aggregate.
/// Defined in Application layer so services can query the database
/// without depending on Infrastructure.
/// </summary>
public interface IPremiseDbContext
{
    /// <summary>Premise entity set.</summary>
    DbSet<Premise> Premises { get; }

    /// <summary>Persists all pending changes to the database.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
