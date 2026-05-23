namespace Bouw.API.Persistence.Entities;

public sealed class Document
{
    private Document() { }

    internal Document(
        Guid sessionId,
        string kind,
        string title,
        string contentMarkdown,
        string createdBy,
        DateTimeOffset createdAt
    )
    {
        this.Id = Guid.NewGuid();
        this.SessionId = sessionId;
        this.Kind = kind;
        this.Title = title;
        this.ContentMarkdown = contentMarkdown;
        this.Version = 1;
        this.CreatedBy = createdBy;
        this.CreatedAt = createdAt;
        this.UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public string Kind { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string ContentMarkdown { get; private set; } = string.Empty;
    public int Version { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public WorkflowSession WorkflowSession { get; private set; } = null!;

    public void UpdateContent(string contentMarkdown, DateTimeOffset updatedAt)
    {
        this.ContentMarkdown = contentMarkdown;
        this.Version++;
        this.UpdatedAt = updatedAt;
    }
}
