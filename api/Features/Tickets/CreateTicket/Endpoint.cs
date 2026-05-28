using Microsoft.AspNetCore.Http.HttpResults;

namespace Bouw.API.Features.Tickets.CreateTicket;

public static class Endpoint
{
    public static void MapCreateTicket(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapPost(
                "/tickets",
                async Task<Results<Created<TicketResponse>, BadRequest>> (
                    CreateTicketRequest request,
                    Handler handler,
                    CancellationToken cancellationToken
                ) =>
                {
                    try
                    {
                        var ticket = await handler
                            .HandleAsync(request, cancellationToken)
                            .ConfigureAwait(false);

                        return TypedResults.Created($"/tickets/{ticket.Id}", ticket);
                    }
                    catch (InvalidTicketInputException)
                    {
                        return TypedResults.BadRequest();
                    }
                }
            )
            .WithName("CreateTicket");
    }
}
