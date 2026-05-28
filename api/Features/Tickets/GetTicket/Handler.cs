using Bouw.API.Persistence;
using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bouw.API.Features.Tickets.GetTicket;

public sealed class Handler
{
    private readonly BouwDbContext db;

    public Handler(BouwDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);

        this.db = db;
    }

    public async Task<TicketResponse?> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await this
            .db.Tickets.SingleOrDefaultAsync(ticket => ticket.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return ticket is null ? null : Map(ticket);
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
