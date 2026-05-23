using Bouw.API.Persistence;
using Bouw.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Bouw")
    ?? throw new InvalidOperationException("Missing required connection string 'ConnectionStrings:Bouw'.");

builder.Services.AddOpenApi();
builder.Services.AddDbContext<BouwDbContext>(options =>
    options
        .UseNpgsql(connectionString)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapFeatures();

await app.RunAsync().ConfigureAwait(false);
