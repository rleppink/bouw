namespace Bouw.API.Features.Workflows.GetWorkflow;

public sealed record WorkflowResponse(
    Guid Id,
    string Name,
    string Description,
    string Status,
    IReadOnlyCollection<WorkflowStepResponse> Steps
);

public sealed record WorkflowStepResponse(
    Guid Id,
    string Key,
    string Name,
    int Position,
    IReadOnlyCollection<WorkflowActionResponse> Actions
);

public sealed record WorkflowActionResponse(
    Guid Id,
    string Key,
    string Type,
    int Position,
    string ConfigJson
);
