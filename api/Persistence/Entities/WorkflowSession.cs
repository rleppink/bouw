namespace Bouw.API.Persistence.Entities;

public sealed class WorkflowSession
{
    private readonly List<SessionStep> steps = [];
    private readonly List<ActionRun> actionRuns = [];
    private readonly List<Document> documents = [];

    private WorkflowSession() { }

    internal WorkflowSession(
        Guid workflowId,
        string title,
        SessionStatus status,
        string? currentStepKey,
        DateTimeOffset createdAt
    )
    {
        this.Id = Guid.NewGuid();
        this.WorkflowId = workflowId;
        this.Title = title;
        this.Status = status;
        this.CurrentStepKey = currentStepKey;
        this.CreatedAt = createdAt;
        this.UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid WorkflowId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public SessionStatus Status { get; private set; }
    public string? CurrentStepKey { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Workflow Workflow { get; private set; } = null!;
    public IReadOnlyCollection<SessionStep> Steps => this.steps;
    public IReadOnlyCollection<ActionRun> ActionRuns => this.actionRuns;
    public IReadOnlyCollection<Document> Documents => this.documents;

    internal SessionStep AddStep(string stepKey, SessionStepStatus status)
    {
        var step = new SessionStep(this.Id, stepKey, status);
        this.steps.Add(step);
        return step;
    }

    public ActionRun StartAction(
        string stepKey,
        string actionKey,
        string inputJson,
        DateTimeOffset startedAt
    )
    {
        var actionRun = new ActionRun(this.Id, stepKey, actionKey, inputJson, startedAt);
        this.actionRuns.Add(actionRun);
        this.UpdatedAt = startedAt;
        return actionRun;
    }

    public Document CreateDocument(
        string kind,
        string title,
        string contentMarkdown,
        string createdBy,
        DateTimeOffset createdAt
    )
    {
        var document = new Document(this.Id, kind, title, contentMarkdown, createdBy, createdAt);
        this.documents.Add(document);
        this.UpdatedAt = createdAt;
        return document;
    }
}
