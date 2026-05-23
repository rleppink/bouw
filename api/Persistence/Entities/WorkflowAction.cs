namespace Bouw.API.Persistence.Entities;

public sealed class WorkflowAction
{
    private WorkflowAction() { }

    internal WorkflowAction(
        Guid stepId,
        string key,
        WorkflowActionType type,
        int position,
        string configJson
    )
    {
        this.Id = Guid.NewGuid();
        this.StepId = stepId;
        this.Key = key;
        this.Type = type;
        this.Position = position;
        this.ConfigJson = configJson;
    }

    public Guid Id { get; private set; }
    public Guid StepId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public WorkflowActionType Type { get; private set; }
    public int Position { get; private set; }
    public string ConfigJson { get; private set; } = "{}";
    public WorkflowStep WorkflowStep { get; private set; } = null!;
}
