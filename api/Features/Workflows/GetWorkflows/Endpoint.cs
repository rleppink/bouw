using Microsoft.AspNetCore.Http.HttpResults;

namespace Bouw.API.Features.Workflows.GetWorkflows;

public static class Endpoint
{
    public static void MapGetWorkflows(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet(
                "/workflows",
                async Task<Ok<IReadOnlyCollection<WorkflowResponse>>> (
                    Handler handler,
                    CancellationToken cancellationToken
                ) =>
                {
                    var workflows = await handler
                        .HandleAsync(cancellationToken)
                        .ConfigureAwait(false);

                    return TypedResults.Ok(workflows);
                }
            )
            .WithName("GetWorkflows");
    }
}
