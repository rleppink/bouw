namespace Bouw.API.Features.Workflows.GetWorkflow;

public sealed record WorkflowStepResponse(
    Guid Id,
    string Key,
    string Name,
    int Position,
    IReadOnlyCollection<WorkflowActionResponse> Actions
);
