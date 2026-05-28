namespace Bouw.API.Features.Tickets.CreateTicket;

public sealed record CreateTicketRequest(string? UserInput);

public sealed record TicketResponse(
    Guid Id,
    string Title,
    string UserInput,
    string Status,
    string LlmResponse,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
