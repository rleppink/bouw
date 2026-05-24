using Bouw.API.Persistence.Entities;

namespace Bouw.API.Persistence.Seeding;

internal sealed record WorkflowSeedDefinition(
    string Key,
    string Name,
    string Description,
    WorkflowStatus Status,
    IReadOnlyCollection<WorkflowStepSeedDefinition> Steps
);
