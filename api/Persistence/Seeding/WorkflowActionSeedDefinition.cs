using Bouw.API.Persistence.Entities;

namespace Bouw.API.Persistence.Seeding;

internal sealed record WorkflowActionSeedDefinition(
    string Key,
    WorkflowActionType Type,
    int Position,
    string ConfigJson
);
