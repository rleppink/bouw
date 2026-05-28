using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bouw.API.Persistence;

public sealed class BouwDbContext(DbContextOptions<BouwDbContext> options) : DbContext(options)
{
    public DbSet<Workflow> Workflows => this.Set<Workflow>();
    public DbSet<WorkflowStep> Steps => this.Set<WorkflowStep>();
    public DbSet<WorkflowAction> WorkflowActions => this.Set<WorkflowAction>();
    public DbSet<WorkflowSession> Sessions => this.Set<WorkflowSession>();
    public DbSet<SessionStep> SessionSteps => this.Set<SessionStep>();
    public DbSet<ActionRun> ActionRuns => this.Set<ActionRun>();
    public DbSet<Document> Documents => this.Set<Document>();
    public DbSet<Ticket> Tickets => this.Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BouwDbContext).Assembly);
    }
}
