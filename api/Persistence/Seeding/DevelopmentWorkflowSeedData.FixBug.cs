using Bouw.API.Persistence.Entities;

namespace Bouw.API.Persistence.Seeding;

internal static partial class DevelopmentWorkflowSeedData
{
    private static WorkflowSeedDefinition CreateFixBugWorkflow() =>
        new(
            "fix-bug",
            "Fix a Bug",
            "A debugging workflow that isolates the failure before changing code.",
            WorkflowStatus.Active,
            [CreateFixBugReproduceStep(), CreateFixBugDiagnoseStep(), CreateFixBugRepairStep()]
        );

    private static WorkflowStepSeedDefinition CreateFixBugReproduceStep() =>
        new(
            "reproduce",
            "Reproduce the failure",
            10,
            [
                new(
                    "capture_symptoms",
                    WorkflowActionType.AskUserInput,
                    10,
                    """
                    {
                      "fields": ["observed_behavior", "expected_behavior", "reproduction_steps"],
                      "prompt": "Capture the failing behavior, expected behavior, and the smallest reliable reproduction."
                    }
                    """
                ),
                new(
                    "write_failure_note",
                    WorkflowActionType.EditDocument,
                    20,
                    """
                    {
                      "documentKind": "bug_report",
                      "mode": "replace"
                    }
                    """
                ),
            ]
        );

    private static WorkflowStepSeedDefinition CreateFixBugDiagnoseStep() =>
        new(
            "diagnose",
            "Diagnose root cause",
            20,
            [
                new(
                    "inspect_evidence",
                    WorkflowActionType.CallLlm,
                    10,
                    """
                    {
                      "prompt": "Analyze the reproduction, relevant code, and logs. Propose the most likely root cause and the narrowest fix.",
                      "requiredInputs": ["bug_report"]
                    }
                    """
                ),
                new(
                    "persist_diagnosis",
                    WorkflowActionType.EditDocument,
                    20,
                    """
                    {
                      "documentKind": "diagnosis",
                      "mode": "replace"
                    }
                    """
                ),
            ]
        );

    private static WorkflowStepSeedDefinition CreateFixBugRepairStep() =>
        new(
            "repair",
            "Repair and prove",
            30,
            [
                new(
                    "apply_fix",
                    WorkflowActionType.CallLlm,
                    10,
                    """
                    {
                      "prompt": "Apply the smallest code change that addresses the diagnosed root cause.",
                      "requiredInputs": ["diagnosis"]
                    }
                    """
                ),
                new(
                    "confirm_regression_test",
                    WorkflowActionType.AskUserInput,
                    20,
                    """
                    {
                      "fields": ["regression_test", "tests_run", "remaining_uncertainty"],
                      "prompt": "Confirm the regression coverage and verification result before closing the bug."
                    }
                    """
                ),
            ]
        );
}
