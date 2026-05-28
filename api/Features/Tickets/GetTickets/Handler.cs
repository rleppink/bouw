using Bouw.API.Persistence;
using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bouw.API.Features.Tickets.GetTickets;

public sealed class Handler
{
    private readonly BouwDbContext db;

    public Handler(BouwDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);

        this.db = db;
    }

    public async Task<IReadOnlyCollection<TicketResponse>> HandleAsync(
        CancellationToken cancellationToken
    )
    {
        var tickets = await this
            .db.Tickets.OrderByDescending(ticket => ticket.CreatedAt)
            .ThenByDescending(ticket => ticket.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return tickets.Select(Map).ToArray();
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
