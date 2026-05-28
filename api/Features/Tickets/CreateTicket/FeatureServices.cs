using Microsoft.Extensions.DependencyInjection;

namespace Bouw.API.Features.Tickets.CreateTicket;

public static class FeatureServices
{
    public static void AddCreateTicket(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<Handler>();
    }
}
