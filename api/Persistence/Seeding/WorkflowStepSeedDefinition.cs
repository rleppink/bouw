namespace Bouw.API.Persistence.Seeding;

internal sealed record WorkflowStepSeedDefinition(
    string Key,
    string Name,
    int Position,
    IReadOnlyCollection<WorkflowActionSeedDefinition> Actions
);
