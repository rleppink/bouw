using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bouw.API.Persistence.Seeding;

public static class DevelopmentDatabaseSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var scope = services.CreateAsyncScope();
        try
        {
            await SeedScopedAsync(scope.ServiceProvider, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await scope.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static async Task SeedScopedAsync(
        IServiceProvider services,
        CancellationToken cancellationToken
    )
    {
        var db = services.GetRequiredService<BouwDbContext>();

        await db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        if (await HasSeededWorkflowDataAsync(db, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var transaction = await db
            .Database.BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            foreach (var definition in DevelopmentWorkflowSeedData.Workflows)
            {
                db.Workflows.Add(CreateWorkflow(definition));
            }

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await transaction.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static async Task<bool> HasSeededWorkflowDataAsync(
        BouwDbContext db,
        CancellationToken cancellationToken
    )
    {
        var workflowCount = await db.Workflows.CountAsync(cancellationToken).ConfigureAwait(false);
        var stepCount = await db.Steps.CountAsync(cancellationToken).ConfigureAwait(false);
        var actionCount = await db
            .WorkflowActions.CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return workflowCount > 0 || stepCount > 0 || actionCount > 0;
    }

    private static Workflow CreateWorkflow(WorkflowSeedDefinition definition)
    {
        var workflow = new Workflow(definition.Name, definition.Description, definition.Status);

        foreach (var stepDefinition in definition.Steps)
        {
            var step = workflow.AddStep(
                stepDefinition.Key,
                stepDefinition.Name,
                stepDefinition.Position
            );

            foreach (var actionDefinition in stepDefinition.Actions)
            {
                step.AddAction(
                    actionDefinition.Key,
                    actionDefinition.Type,
                    actionDefinition.Position,
                    actionDefinition.ConfigJson
                );
            }
        }

        return workflow;
    }
}
