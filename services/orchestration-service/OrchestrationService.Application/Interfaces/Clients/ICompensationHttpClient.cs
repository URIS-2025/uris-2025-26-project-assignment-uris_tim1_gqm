namespace OrchestrationService.Application.Interfaces.Clients;

public interface ICompensationHttpClient
{
    /// <summary>
    /// Calls the compensation endpoint. Returns true on 2xx, false otherwise. Never throws.
    /// </summary>
    Task<bool> CallAsync(string endpoint, string payload);
}
