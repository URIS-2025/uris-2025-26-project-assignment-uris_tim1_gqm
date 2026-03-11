namespace OrchestrationService.Application.DTOs;

public class RecordStepRequest
{
    public string StepName { get; set; } = string.Empty;
    public string CompensationEndpoint { get; set; } = string.Empty;
    public string CompensationPayload { get; set; } = string.Empty;
}
