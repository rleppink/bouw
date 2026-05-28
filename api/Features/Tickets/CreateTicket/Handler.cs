using Bouw.API.Infrastructure.Tickets;
using Bouw.API.Persistence;
using Bouw.API.Persistence.Entities;

namespace Bouw.API.Features.Tickets.CreateTicket;

public sealed class Handler
{
    private readonly BouwDbContext db;
    private readonly ITicketProcessor ticketProcessor;
    private readonly TimeProvider timeProvider;

    public Handler(BouwDbContext db, ITicketProcessor ticketProcessor, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(ticketProcessor);
        ArgumentNullException.ThrowIfNull(timeProvider);

        this.db = db;
        this.ticketProcessor = ticketProcessor;
        this.timeProvider = timeProvider;
    }

    public async Task<TicketResponse> HandleAsync(
        CreateTicketRequest request,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.UserInput))
        {
            throw new InvalidTicketInputException();
        }

        var now = this.timeProvider.GetUtcNow();
        var ticket = new Ticket(request.UserInput, now);

        this.db.Tickets.Add(ticket);
        await this.db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        this.ticketProcessor.Enqueue(ticket.Id);

        return Map(ticket);
    }

    private static TicketResponse Map(Ticket ticket) =>
        new(
            ticket.Id,
            ticket.Title,
            ticket.UserInput,
            ToResponseValue(ticket.Status),
            ticket.LlmResponse,
            ticket.CreatedAt,
            ticket.UpdatedAt
        );

    private static string ToResponseValue(TicketStatus status) =>
        status switch
        {
            TicketStatus.Pending => "pending",
            TicketStatus.Completed => "completed",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, message: null),
        };
}
