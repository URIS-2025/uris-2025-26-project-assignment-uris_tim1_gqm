using System.Text.Json.Serialization;

namespace Shared.ErrorHandling;

/// <summary>
/// Standardized error response format used across all microservices.
/// </summary>
public class StandardErrorResponse
{
    /// <summary>
    /// Machine-readable error type (e.g., "validation_error", "not_found", "conflict").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable summary of the error.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Field-level error details. Keys are field names, values are arrays of error messages.
    /// Empty dictionary when no field-level details are available.
    /// </summary>
    [JsonPropertyName("errors")]
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public static StandardErrorResponse Create(string type, string title, Dictionary<string, string[]>? errors = null)
    {
        return new StandardErrorResponse
        {
            Type = type,
            Title = title,
            Errors = errors ?? new Dictionary<string, string[]>()
        };
    }
}

/// <summary>
/// Well-known error type constants for consistent usage across all services.
/// </summary>
public static class ErrorTypes
{
    public const string ValidationError = "validation_error";
    public const string BadRequest = "bad_request";
    public const string NotFound = "not_found";
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";
    public const string Conflict = "conflict";
    public const string UnprocessableEntity = "unprocessable_entity";
    public const string InternalServerError = "internal_server_error";
}
