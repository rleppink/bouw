namespace Bouw.API.Features.Tickets.GetTicket;

public sealed record TicketResponse(
    Guid Id,
    string Title,
    string UserInput,
    string Status,
    string LlmResponse,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
