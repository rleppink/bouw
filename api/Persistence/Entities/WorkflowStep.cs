namespace Bouw.API.Persistence.Entities;

public sealed class WorkflowStep
{
    private readonly List<WorkflowAction> actions = [];

    private WorkflowStep() { }

    internal WorkflowStep(Guid workflowId, string key, string name, int position)
    {
        this.Id = Guid.NewGuid();
        this.WorkflowId = workflowId;
        this.Key = key;
        this.Name = name;
        this.Position = position;
    }

    public Guid Id { get; private set; }
    public Guid WorkflowId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int Position { get; private set; }
    public Workflow Workflow { get; private set; } = null!;
    public IReadOnlyCollection<WorkflowAction> Actions => this.actions;

    public WorkflowAction AddAction(
        string key,
        WorkflowActionType type,
        int position,
        string configJson
    )
    {
        if (this.actions.Exists(action => string.Equals(action.Key, key, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"WorkflowStep '{this.Key}' already has action '{key}'."
            );
        }

        var action = new WorkflowAction(this.Id, key, type, position, configJson);
        this.actions.Add(action);
        return action;
    }
}
