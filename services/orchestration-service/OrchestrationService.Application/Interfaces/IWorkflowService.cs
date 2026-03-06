using OrchestrationService.Application.DTOs;

namespace OrchestrationService.Application.Interfaces;

public interface IWorkflowService
{
    Task<SagaWorkflowResponse> StartWorkflowAsync(StartWorkflowRequest request);
    Task<SagaWorkflowResponse> GetWorkflowAsync(Guid goalId);
    Task<SagaWorkflowResponse> RecordStepAsync(Guid goalId, RecordStepRequest request);
    Task<SagaWorkflowResponse> CancelWorkflowAsync(Guid goalId);
    Task<IEnumerable<SagaStepResponse>> GetStepsAsync(Guid goalId);
}
