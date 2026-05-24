namespace Bouw.API.Features.Workflows.GetWorkflows;

public sealed record WorkflowActionResponse(
    Guid Id,
    string Key,
    string Type,
    int Position,
    string ConfigJson
);
