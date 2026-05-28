using Microsoft.AspNetCore.Http.HttpResults;

namespace Bouw.API.Features.Tickets.GetTickets;

public static class Endpoint
{
    public static void MapGetTickets(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet(
                "/tickets",
                async Task<Ok<IReadOnlyCollection<TicketResponse>>> (
                    Handler handler,
                    CancellationToken cancellationToken
                ) =>
                {
                    var tickets = await handler
                        .HandleAsync(cancellationToken)
                        .ConfigureAwait(false);

                    return TypedResults.Ok(tickets);
                }
            )
            .WithName("GetTickets");
    }
}
