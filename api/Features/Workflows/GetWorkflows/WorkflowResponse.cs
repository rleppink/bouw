namespace Bouw.API.Features.Workflows.GetWorkflows;

public sealed record WorkflowResponse(
    Guid Id,
    string Name,
    string Description,
    string Status,
    IReadOnlyCollection<WorkflowStepResponse> Steps
);
