using Bouw.API.Persistence.Entities;

namespace Bouw.API.Persistence.Seeding;

internal static partial class DevelopmentWorkflowSeedData
{
    private static WorkflowSeedDefinition CreateRefactorSafelyWorkflow() =>
        new(
            "refactor-safely",
            "Refactor Safely",
            "A behavior-preserving workflow for simplifying code with explicit guardrails.",
            WorkflowStatus.Active,
            [
                CreateRefactorSafelyScopeStep(),
                CreateRefactorSafelyGuardStep(),
                CreateRefactorSafelyReshapeStep(),
            ]
        );

    private static WorkflowStepSeedDefinition CreateRefactorSafelyScopeStep() =>
        new(
            "scope",
            "Scope the refactor",
            10,
            [
                new(
                    "capture_boundaries",
                    WorkflowActionType.AskUserInput,
                    10,
                    """
                    {
                      "fields": ["target_area", "pain_point", "behavior_to_preserve"],
                      "prompt": "Define the refactor boundary, the code smell being addressed, and the behavior that must not change."
                    }
                    """
                ),
                new(
                    "summarize_constraints",
                    WorkflowActionType.CallLlm,
                    20,
                    """
                    {
                      "prompt": "Summarize the refactor constraints and identify the tests or checks needed before editing.",
                      "outputDocumentKind": "refactor_brief"
                    }
                    """
                ),
            ]
        );

    private static WorkflowStepSeedDefinition CreateRefactorSafelyGuardStep() =>
        new(
            "guard",
            "Establish guardrails",
            20,
            [
                new(
                    "propose_checks",
                    WorkflowActionType.CallLlm,
                    10,
                    """
                    {
                      "prompt": "Identify focused tests, snapshots, or manual checks that will catch behavior changes.",
                      "requiredInputs": ["refactor_brief"]
                    }
                    """
                ),
                new(
                    "record_guardrails",
                    WorkflowActionType.EditDocument,
                    20,
                    """
                    {
                      "documentKind": "refactor_guardrails",
                      "mode": "replace"
                    }
                    """
                ),
            ]
        );

    private static WorkflowStepSeedDefinition CreateRefactorSafelyReshapeStep() =>
        new(
            "reshape",
            "Reshape the code",
            30,
            [
                new(
                    "apply_refactor",
                    WorkflowActionType.CallLlm,
                    10,
                    """
                    {
                      "prompt": "Refactor within the agreed boundary without changing externally visible behavior.",
                      "requiredInputs": ["refactor_guardrails"]
                    }
                    """
                ),
                new(
                    "verify_behavior",
                    WorkflowActionType.AskUserInput,
                    20,
                    """
                    {
                      "fields": ["checks_run", "diff_reviewed", "behavior_changed"],
                      "prompt": "Confirm the verification evidence and whether any behavior changed."
                    }
                    """
                ),
            ]
        );
}
