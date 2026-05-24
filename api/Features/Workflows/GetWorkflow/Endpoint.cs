using Microsoft.AspNetCore.Http.HttpResults;

namespace Bouw.API.Features.Workflows.GetWorkflow;

public static class Endpoint
{
    public static void MapGetWorkflow(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet(
                "/workflows/{key}",
                async Task<Results<Ok<WorkflowResponse>, NotFound>> (
                    string key,
                    GetWorkflowHandler handler,
                    CancellationToken cancellationToken
                ) =>
                {
                    var workflow = await handler
                        .HandleAsync(key, cancellationToken)
                        .ConfigureAwait(false);

                    return workflow is null ? TypedResults.NotFound() : TypedResults.Ok(workflow);
                }
            )
            .WithName("GetWorkflow");
    }
}
