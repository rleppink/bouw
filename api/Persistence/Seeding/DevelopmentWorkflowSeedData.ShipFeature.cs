using Bouw.API.Persistence.Entities;

namespace Bouw.API.Persistence.Seeding;

internal static partial class DevelopmentWorkflowSeedData
{
    private static WorkflowSeedDefinition CreateShipFeatureWorkflow() =>
        new(
            "ship-feature",
            "Ship a Feature",
            "A focused software-delivery workflow from problem framing to implementation review.",
            WorkflowStatus.Active,
            [
                CreateShipFeatureFrameStep(),
                CreateShipFeaturePlanStep(),
                CreateShipFeatureBuildStep(),
                CreateShipFeatureReviewStep(),
            ]
        );

    private static WorkflowStepSeedDefinition CreateShipFeatureFrameStep() =>
        new(
            "frame",
            "Frame the work",
            10,
            [
                new(
                    "capture_context",
                    WorkflowActionType.AskUserInput,
                    10,
                    """
                    {
                      "fields": ["goal", "constraints", "definition_of_done"],
                      "prompt": "Capture the feature goal, hard constraints, and the smallest acceptable outcome."
                    }
                    """
                ),
                new(
                    "synthesize_brief",
                    WorkflowActionType.CallLlm,
                    20,
                    """
                    {
                      "prompt": "Turn the captured context into a concise engineering brief with risks and open questions.",
                      "outputDocumentKind": "feature_brief"
                    }
                    """
                ),
            ]
        );

    private static WorkflowStepSeedDefinition CreateShipFeaturePlanStep() =>
        new(
            "plan",
            "Plan the change",
            20,
            [
                new(
                    "draft_plan",
                    WorkflowActionType.CallLlm,
                    10,
                    """
                    {
                      "prompt": "Create a small implementation plan that respects the existing architecture.",
                      "requiredInputs": ["feature_brief"]
                    }
                    """
                ),
                new(
                    "persist_plan",
                    WorkflowActionType.EditDocument,
                    20,
                    """
                    {
                      "documentKind": "implementation_plan",
                      "mode": "replace"
                    }
                    """
                ),
            ]
        );

    private static WorkflowStepSeedDefinition CreateShipFeatureBuildStep() =>
        new(
            "build",
            "Build the slice",
            30,
            [
                new(
                    "apply_changes",
                    WorkflowActionType.CallLlm,
                    10,
                    """
                    {
                      "prompt": "Implement the planned change in the smallest coherent set of files.",
                      "requiredInputs": ["implementation_plan"]
                    }
                    """
                ),
                new(
                    "record_notes",
                    WorkflowActionType.EditDocument,
                    20,
                    """
                    {
                      "documentKind": "change_notes",
                      "mode": "append"
                    }
                    """
                ),
            ]
        );

    private static WorkflowStepSeedDefinition CreateShipFeatureReviewStep() =>
        new(
            "review",
            "Review and verify",
            40,
            [
                new(
                    "verification_checklist",
                    WorkflowActionType.AskUserInput,
                    10,
                    """
                    {
                      "fields": ["tests_run", "known_risks", "ship_decision"],
                      "prompt": "Confirm verification results and decide whether the feature is ready to ship."
                    }
                    """
                ),
            ]
        );
}
