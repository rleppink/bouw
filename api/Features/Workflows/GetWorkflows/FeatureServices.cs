using Microsoft.Extensions.DependencyInjection;

namespace Bouw.API.Features.Workflows.GetWorkflows;

public static class FeatureServices
{
    public static void AddGetWorkflows(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<Handler>();
    }
}
