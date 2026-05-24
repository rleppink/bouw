using Microsoft.Extensions.DependencyInjection;

namespace Bouw.API.Features.Workflows.GetWorkflow;

public static class FeatureServices
{
    public static void AddGetWorkflow(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<GetWorkflowHandler>();
    }
}
