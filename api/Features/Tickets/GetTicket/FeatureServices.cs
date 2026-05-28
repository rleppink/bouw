using Microsoft.Extensions.DependencyInjection;

namespace Bouw.API.Features.Tickets.GetTicket;

public static class FeatureServices
{
    public static void AddGetTicket(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<Handler>();
    }
}
