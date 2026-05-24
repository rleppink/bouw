namespace Bouw.API.Features.Workflows.GetWorkflow;

public sealed record WorkflowResponse(
    Guid Id,
    string Key,
    string Name,
    string Description,
    string Status,
    IReadOnlyCollection<WorkflowStepResponse> Steps
);
