namespace Bouw.API.Features.Tickets.GetTickets;

public sealed record TicketResponse(
    Guid Id,
    string Title,
    string UserInput,
    string Status,
    string LlmResponse,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
