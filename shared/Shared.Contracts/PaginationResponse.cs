namespace Shared.Contracts;

public record PaginationResponse<T>
{
    public IEnumerable<T> Items { get; init; } = new List<T>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
}
