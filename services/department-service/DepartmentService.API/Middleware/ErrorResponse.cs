namespace DepartmentService.API.Middleware;

public record ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ValidationError>? Errors { get; set; }
}

public record ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
