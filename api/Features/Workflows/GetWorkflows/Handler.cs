using Bouw.API.Persistence;
using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bouw.API.Features.Workflows.GetWorkflows;

public sealed class Handler
{
    private readonly BouwDbContext db;

    public Handler(BouwDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);

        this.db = db;
    }

    public async Task<IReadOnlyCollection<WorkflowResponse>> HandleAsync(
        CancellationToken cancellationToken
    )
    {
        var workflows = await this
            .db.Workflows.Include(workflow => workflow.Steps)
                .ThenInclude(step => step.Actions)
            .OrderBy(workflow => workflow.Name)
            .ThenBy(workflow => workflow.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return workflows.Select(Map).ToArray();
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
