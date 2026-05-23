using Microsoft.EntityFrameworkCore;

namespace Bouw.API.Persistence;

public sealed class BouwDbContext(DbContextOptions<BouwDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BouwDbContext).Assembly);
    }
}
