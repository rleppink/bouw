using Bouw.API.Persistence;
using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bouw.API.Features.Workflows.GetWorkflow;

public sealed class GetWorkflowHandler
{
    private readonly BouwDbContext db;

    public GetWorkflowHandler(BouwDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);

        this.db = db;
    }

    public async Task<WorkflowResponse?> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var workflow = await this
            .db.Workflows.Include(workflow => workflow.Steps)
                .ThenInclude(step => step.Actions)
            .SingleOrDefaultAsync(workflow => workflow.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return workflow is null ? null : Map(workflow);
    }

    private static WorkflowResponse Map(Workflow workflow) =>
        new(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            ToResponseValue(workflow.Status),
            workflow.Steps.OrderBy(step => step.Position).Select(MapStep).ToArray()
        );

    private static WorkflowStepResponse MapStep(WorkflowStep step) =>
        new(
            step.Id,
            step.Key,
            step.Name,
            step.Position,
            step.Actions.OrderBy(action => action.Position).Select(MapAction).ToArray()
        );

    private static WorkflowActionResponse MapAction(WorkflowAction action) =>
        new(
            action.Id,
            action.Key,
            ToResponseValue(action.Type),
            action.Position,
            action.ConfigJson
        );

    private static string ToResponseValue(WorkflowStatus status) =>
        status switch
        {
            WorkflowStatus.Draft => "draft",
            WorkflowStatus.Active => "active",
            WorkflowStatus.Archived => "archived",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, message: null),
        };

    private static string ToResponseValue(WorkflowActionType actionType) =>
        actionType switch
        {
            WorkflowActionType.AskUserInput => "ask_user_input",
            WorkflowActionType.CallLlm => "call_llm",
            WorkflowActionType.EditDocument => "edit_document",
            _ => throw new ArgumentOutOfRangeException(
                nameof(actionType),
                actionType,
                message: null
            ),
        };
}
