namespace Bouw.API.Persistence.Entities;

public sealed class ActionRun
{
    private ActionRun() { }

    internal ActionRun(
        Guid sessionId,
        string stepKey,
        string actionKey,
        string inputJson,
        DateTimeOffset startedAt
    )
    {
        this.Id = Guid.NewGuid();
        this.SessionId = sessionId;
        this.StepKey = stepKey;
        this.ActionKey = actionKey;
        this.Status = ActionRunStatus.Running;
        this.InputJson = inputJson;
        this.StartedAt = startedAt;
    }

    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public string StepKey { get; private set; } = string.Empty;
    public string ActionKey { get; private set; } = string.Empty;
    public ActionRunStatus Status { get; private set; }
    public string InputJson { get; private set; } = "{}";
    public string? OutputJson { get; private set; }
    public string? Error { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public WorkflowSession WorkflowSession { get; private set; } = null!;

    public void Complete(string outputJson, DateTimeOffset completedAt)
    {
        this.Status = ActionRunStatus.Complete;
        this.OutputJson = outputJson;
        this.CompletedAt = completedAt;
    }

    public void Fail(string error, DateTimeOffset completedAt)
    {
        this.Status = ActionRunStatus.Failed;
        this.Error = error;
        this.CompletedAt = completedAt;
    }
}
