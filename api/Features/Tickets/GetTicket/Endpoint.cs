using Microsoft.AspNetCore.Http.HttpResults;

namespace Bouw.API.Features.Tickets.GetTicket;

public static class Endpoint
{
    public static void MapGetTicket(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet(
                "/tickets/{id:guid}",
                async Task<Results<Ok<TicketResponse>, NotFound>> (
                    Guid id,
                    Handler handler,
                    CancellationToken cancellationToken
                ) =>
                {
                    var ticket = await handler
                        .HandleAsync(id, cancellationToken)
                        .ConfigureAwait(false);

                    return ticket is null ? TypedResults.NotFound() : TypedResults.Ok(ticket);
                }
            )
            .WithName("GetTicket");
    }
}
