using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bouw.API.Persistence.Configurations;

internal static class WorkflowValueConverters
{
    public static ValueConverter<WorkflowStatus, string> WorkflowStatus { get; } =
        new(status => ToDatabase(status), value => ToWorkflowStatus(value));

    public static ValueConverter<WorkflowActionType, string> WorkflowActionType { get; } =
        new(actionType => ToDatabase(actionType), value => ToWorkflowActionType(value));

    public static ValueConverter<SessionStatus, string> SessionStatus { get; } =
        new(status => ToDatabase(status), value => ToSessionStatus(value));

    public static ValueConverter<SessionStepStatus, string> SessionStepStatus { get; } =
        new(status => ToDatabase(status), value => ToSessionStepStatus(value));

    public static ValueConverter<ActionRunStatus, string> ActionRunStatus { get; } =
        new(status => ToDatabase(status), value => ToActionRunStatus(value));

    private static string ToDatabase(WorkflowStatus status) =>
        status switch
        {
            Entities.WorkflowStatus.Draft => "draft",
            Entities.WorkflowStatus.Active => "active",
            Entities.WorkflowStatus.Archived => "archived",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, message: null),
        };

    private static WorkflowStatus ToWorkflowStatus(string value) =>
        value switch
        {
            "draft" => Entities.WorkflowStatus.Draft,
            "active" => Entities.WorkflowStatus.Active,
            "archived" => Entities.WorkflowStatus.Archived,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, message: null),
        };

    private static string ToDatabase(WorkflowActionType actionType) =>
        actionType switch
        {
            Entities.WorkflowActionType.AskUserInput => "ask_user_input",
            Entities.WorkflowActionType.CallLlm => "call_llm",
            Entities.WorkflowActionType.EditDocument => "edit_document",
            _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, message: null),
        };

    private static WorkflowActionType ToWorkflowActionType(string value) =>
        value switch
        {
            "ask_user_input" => Entities.WorkflowActionType.AskUserInput,
            "call_llm" => Entities.WorkflowActionType.CallLlm,
            "edit_document" => Entities.WorkflowActionType.EditDocument,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, message: null),
        };

    private static string ToDatabase(SessionStatus status) =>
        status switch
        {
            Entities.SessionStatus.Draft => "draft",
            Entities.SessionStatus.Active => "active",
            Entities.SessionStatus.WaitingForUser => "waiting_for_user",
            Entities.SessionStatus.Complete => "complete",
            Entities.SessionStatus.Failed => "failed",
            Entities.SessionStatus.Archived => "archived",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, message: null),
        };

    private static SessionStatus ToSessionStatus(string value) =>
        value switch
        {
            "draft" => Entities.SessionStatus.Draft,
            "active" => Entities.SessionStatus.Active,
            "waiting_for_user" => Entities.SessionStatus.WaitingForUser,
            "complete" => Entities.SessionStatus.Complete,
            "failed" => Entities.SessionStatus.Failed,
            "archived" => Entities.SessionStatus.Archived,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, message: null),
        };

    private static string ToDatabase(SessionStepStatus status) =>
        status switch
        {
            Entities.SessionStepStatus.Pending => "pending",
            Entities.SessionStepStatus.Active => "active",
            Entities.SessionStepStatus.Complete => "complete",
            Entities.SessionStepStatus.Failed => "failed",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, message: null),
        };

    private static SessionStepStatus ToSessionStepStatus(string value) =>
        value switch
        {
            "pending" => Entities.SessionStepStatus.Pending,
            "active" => Entities.SessionStepStatus.Active,
            "complete" => Entities.SessionStepStatus.Complete,
            "failed" => Entities.SessionStepStatus.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, message: null),
        };

    private static string ToDatabase(ActionRunStatus status) =>
        status switch
        {
            Entities.ActionRunStatus.Pending => "pending",
            Entities.ActionRunStatus.Running => "running",
            Entities.ActionRunStatus.WaitingForUser => "waiting_for_user",
            Entities.ActionRunStatus.Complete => "complete",
            Entities.ActionRunStatus.Failed => "failed",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, message: null),
        };

    private static ActionRunStatus ToActionRunStatus(string value) =>
        value switch
        {
            "pending" => Entities.ActionRunStatus.Pending,
            "running" => Entities.ActionRunStatus.Running,
            "waiting_for_user" => Entities.ActionRunStatus.WaitingForUser,
            "complete" => Entities.ActionRunStatus.Complete,
            "failed" => Entities.ActionRunStatus.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, message: null),
        };
}
