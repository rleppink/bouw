using Microsoft.Extensions.DependencyInjection;

namespace Bouw.API.Features.Tickets.GetTickets;

public static class FeatureServices
{
    public static void AddGetTickets(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<Handler>();
    }
}
