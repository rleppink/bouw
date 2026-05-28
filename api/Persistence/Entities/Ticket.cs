namespace Bouw.API.Persistence.Entities;

public sealed class Ticket
{
    private Ticket() { }

    public Ticket(string userInput, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userInput);

        this.Id = Guid.NewGuid();
        this.Title = DeriveTitle(userInput);
        this.UserInput = userInput;
        this.Status = TicketStatus.Pending;
        this.LlmResponse = string.Empty;
        this.CreatedAt = createdAt;
        this.UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string UserInput { get; private set; } = string.Empty;
    public TicketStatus Status { get; private set; }
    public string LlmResponse { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Complete(string llmResponse, DateTimeOffset completedAt)
    {
        ArgumentNullException.ThrowIfNull(llmResponse);

        this.Status = TicketStatus.Completed;
        this.LlmResponse = llmResponse;
        this.UpdatedAt = completedAt;
    }

    private static string DeriveTitle(string userInput)
    {
        var firstNonEmptyLine = userInput
            .Split(["\r\n", "\n", "\r"], StringSplitOptions.None)
            .Select(line => line.Trim())
            .First(line => line.Length > 0);

        return firstNonEmptyLine.Length <= 80 ? firstNonEmptyLine : firstNonEmptyLine[..80];
    }
}
