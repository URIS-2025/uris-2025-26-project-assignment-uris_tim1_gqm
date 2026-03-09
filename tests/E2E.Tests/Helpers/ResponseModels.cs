using System.Text.Json;
using System.Text.Json.Serialization;

namespace E2E.Tests.Helpers;

// ── Shared JSON shapes ────────────────────────────────────────────────────────

public record PaginatedResponse<T>(
    [property: JsonPropertyName("items")]      IReadOnlyList<T> Items,
    [property: JsonPropertyName("pageNumber")] int PageNumber,
    [property: JsonPropertyName("pageSize")]   int PageSize,
    [property: JsonPropertyName("total")]      int Total);

public record AuditLogDto(
    [property: JsonPropertyName("id")]          Guid   Id,
    [property: JsonPropertyName("actorId")]     Guid   ActorId,
    [property: JsonPropertyName("actorRole")]   string ActorRole,
    [property: JsonPropertyName("service")]     string Service,
    [property: JsonPropertyName("action")]      string Action,
    [property: JsonPropertyName("entityType")]  string EntityType,
    [property: JsonPropertyName("entityId")]    Guid   EntityId,
    [property: JsonPropertyName("timestamp")]   DateTime Timestamp,
    [property: JsonPropertyName("metadata")]    string?  Metadata);

public record GoalDto(
    [property: JsonPropertyName("id")]     Guid   Id,
    [property: JsonPropertyName("focus")]  string Focus,
    [property: JsonPropertyName("status")] string Status);

public record WorkflowDto(
    [property: JsonPropertyName("id")]          Guid         Id,
    [property: JsonPropertyName("goalId")]      Guid         GoalId,
    [property: JsonPropertyName("status")]      string       Status,
    [property: JsonPropertyName("currentStep")] string       CurrentStep,
    [property: JsonPropertyName("steps")]       IReadOnlyList<WorkflowStepDto> Steps);

public record WorkflowStepDto(
    [property: JsonPropertyName("stepName")]  string   StepName,
    [property: JsonPropertyName("status")]    string   Status);

// ── Parsing helpers ───────────────────────────────────────────────────────────

public static class HttpResponseExtensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static async Task<T> ReadAs<T>(this HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _options)
               ?? throw new InvalidOperationException($"Could not deserialize response to {typeof(T).Name}.\nBody: {json}");
    }

    public static async Task<T?> TryReadAs<T>(this HttpResponseMessage response) where T : class
    {
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _options);
    }
}
