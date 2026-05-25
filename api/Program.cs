using System.Reflection;
using Bouw.API.Features.Workflows.GetWorkflow;
using Bouw.API.Features.Workflows.GetWorkflows;
using Bouw.API.Infrastructure.Llm;
using Bouw.API.Persistence;
using Bouw.API.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// The build-time OpenAPI generator runs the app outside our launch profile, so
// appsettings.Development.json is not loaded and no real database connection is needed.
var isOpenApiGeneration = string.Equals(
    Assembly.GetEntryAssembly()?.GetName().Name,
    "GetDocument.Insider",
    StringComparison.Ordinal
);
var connectionString = isOpenApiGeneration switch
{
    true => "Host=localhost;Database=bouw_openapi;Username=openapi;Password=openapi",
    _ => builder.Configuration.GetConnectionString("Bouw")
        ?? throw new InvalidOperationException(
            "Missing required connection string 'ConnectionStrings:Bouw'."
        ),
};

builder.Services.AddGetWorkflow();
builder.Services.AddGetWorkflows();
builder.Services.AddLlmInfrastructure();

builder.Services.AddDbContext<BouwDbContext>(options =>
    options.UseNpgsql(connectionString).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    if (!isOpenApiGeneration)
    {
        await DevelopmentDatabaseSeeder
            .SeedAsync(app.Services, app.Lifetime.ApplicationStopping)
            .ConfigureAwait(false);
    }
}

app.UseHttpsRedirection();

app.MapGetWorkflow();
app.MapGetWorkflows();

await app.RunAsync().ConfigureAwait(false);
