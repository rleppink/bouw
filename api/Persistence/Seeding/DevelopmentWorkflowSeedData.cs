namespace Bouw.API.Persistence.Seeding;

internal static partial class DevelopmentWorkflowSeedData
{
    public static IReadOnlyCollection<WorkflowSeedDefinition> Workflows { get; } =
    [CreateShipFeatureWorkflow(), CreateFixBugWorkflow(), CreateRefactorSafelyWorkflow()];
}
