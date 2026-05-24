namespace Bouw.API.Features.Workflows.GetWorkflow;

public sealed record WorkflowActionResponse(
    Guid Id,
    string Key,
    string Type,
    int Position,
    string ConfigJson
);
