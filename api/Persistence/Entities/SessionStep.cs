namespace Bouw.API.Persistence.Entities;

public sealed class SessionStep
{
    private SessionStep() { }

    internal SessionStep(Guid sessionId, string stepKey, SessionStepStatus status)
    {
        this.Id = Guid.NewGuid();
        this.SessionId = sessionId;
        this.StepKey = stepKey;
        this.Status = status;
    }

    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public string StepKey { get; private set; } = string.Empty;
    public SessionStepStatus Status { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public WorkflowSession WorkflowSession { get; private set; } = null!;

    public void Start(DateTimeOffset startedAt)
    {
        this.Status = SessionStepStatus.Active;
        this.StartedAt = startedAt;
    }

    public void Complete(DateTimeOffset completedAt)
    {
        this.Status = SessionStepStatus.Complete;
        this.CompletedAt = completedAt;
    }
}
