namespace PremiseService.Application.DTOs;

/// <summary>
/// Generic paginated response wrapper.
/// </summary>
public record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int Size,
    int Total);
