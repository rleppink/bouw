using Bouw.API.Features.Workflows.GetWorkflow;
using Bouw.API.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("Bouw")
    ?? throw new InvalidOperationException(
        "Missing required connection string 'ConnectionStrings:Bouw'."
    );

builder.Services.AddGetWorkflow();

builder.Services.AddDbContext<BouwDbContext>(options =>
    options.UseNpgsql(connectionString).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGetWorkflow();

await app.RunAsync().ConfigureAwait(false);
