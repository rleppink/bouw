namespace Bouw.API.Persistence.Entities;

public sealed class Workflow
{
    private readonly List<WorkflowStep> steps = [];

    private Workflow() { }

    public Workflow(string name, string description, WorkflowStatus status)
    {
        this.Id = Guid.NewGuid();
        this.Name = name;
        this.Description = description;
        this.Status = status;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public WorkflowStatus Status { get; private set; }
    public IReadOnlyCollection<WorkflowStep> Steps => this.steps;

    public WorkflowStep AddStep(string key, string name, int position)
    {
        if (this.steps.Exists(step => string.Equals(step.Key, key, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Workflow '{this.Id}' already has step '{key}'.");
        }

        var step = new WorkflowStep(this.Id, key, name, position);
        this.steps.Add(step);
        return step;
    }

    public WorkflowSession StartSession(string title, DateTimeOffset startedAt)
    {
        var orderedSteps = this.steps.OrderBy(step => step.Position).ToArray();
        var firstStepKey = orderedSteps.FirstOrDefault()?.Key;
        var session = new WorkflowSession(
            this.Id,
            title,
            SessionStatus.Active,
            firstStepKey,
            startedAt
        );

        foreach (var step in orderedSteps)
        {
            var sessionStep = session.AddStep(step.Key, SessionStepStatus.Pending);
            if (string.Equals(step.Key, firstStepKey, StringComparison.Ordinal))
            {
                sessionStep.Start(startedAt);
            }
        }

        return session;
    }
}
