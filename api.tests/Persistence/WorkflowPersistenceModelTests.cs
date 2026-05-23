using Bouw.API.Persistence;
using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bouw.API.Tests.Persistence;

public sealed class WorkflowPersistenceModelTests
{
    [Fact]
    public void ModelContainsWorkflowRuntimeAndDocumentTables()
    {
        using var context = CreateContext();

        Assert.Equal("workflows", context.Model.FindEntityType(typeof(Workflow))?.GetTableName());
        Assert.Equal(
            "workflow_steps",
            context.Model.FindEntityType(typeof(WorkflowStep))?.GetTableName()
        );
        Assert.Equal(
            "workflow_actions",
            context.Model.FindEntityType(typeof(WorkflowAction))?.GetTableName()
        );
        Assert.Equal(
            "workflow_sessions",
            context.Model.FindEntityType(typeof(WorkflowSession))?.GetTableName()
        );
        Assert.Equal(
            "session_steps",
            context.Model.FindEntityType(typeof(SessionStep))?.GetTableName()
        );
        Assert.Equal(
            "action_runs",
            context.Model.FindEntityType(typeof(ActionRun))?.GetTableName()
        );
        Assert.Equal("documents", context.Model.FindEntityType(typeof(Document))?.GetTableName());
    }

    [Fact]
    public void JsonPayloadColumnsAreStoredAsJsonb()
    {
        using var context = CreateContext();

        Assert.Equal(
            "jsonb",
            context
                .Model.FindEntityType(typeof(WorkflowAction))
                ?.FindProperty("ConfigJson")
                ?.GetColumnType()
        );
        Assert.Equal(
            "jsonb",
            context
                .Model.FindEntityType(typeof(ActionRun))
                ?.FindProperty("InputJson")
                ?.GetColumnType()
        );
        Assert.Equal(
            "jsonb",
            context
                .Model.FindEntityType(typeof(ActionRun))
                ?.FindProperty("OutputJson")
                ?.GetColumnType()
        );
    }

    [Fact]
    public void EnumValuesUseDocumentVocabulary()
    {
        using var context = CreateContext();

        Assert.Equal(
            "active",
            ConvertProperty(context, typeof(Workflow), "Status", WorkflowStatus.Active)
        );
        Assert.Equal(
            "ask_user_input",
            ConvertProperty(
                context,
                typeof(WorkflowAction),
                "Type",
                WorkflowActionType.AskUserInput
            )
        );
        Assert.Equal(
            "waiting_for_user",
            ConvertProperty(
                context,
                typeof(WorkflowSession),
                "Status",
                SessionStatus.WaitingForUser
            )
        );
    }

    [Fact]
    public void StartingWorkflowCreatesSessionWithOrderedSteps()
    {
        var workflow = new Workflow("frame", "Frame", "Frame the work", WorkflowStatus.Active);
        workflow.AddStep("synthesize", "Synthesize", position: 20);
        workflow.AddStep("interview", "Interview", position: 10);

        var startedAt = new DateTimeOffset(2026, 5, 23, 10, 15, 0, TimeSpan.Zero);
        var session = workflow.StartSession("Ship a feature", startedAt);

        Assert.Equal(workflow.Id, session.WorkflowId);
        Assert.Equal(SessionStatus.Active, session.Status);
        Assert.Equal("interview", session.CurrentStepKey);
        Assert.Equal(startedAt, session.CreatedAt);
        Assert.Equal(["interview", "synthesize"], session.Steps.Select(step => step.StepKey));
        Assert.Equal(SessionStepStatus.Active, session.Steps.First().Status);
        Assert.Equal(startedAt, session.Steps.First().StartedAt);
    }

    [Fact]
    public void DocumentUpdatesIncrementVersion()
    {
        var workflow = new Workflow("plan", "Plan", "Plan the work", WorkflowStatus.Active);
        var session = workflow.StartSession(
            "Plan a feature",
            new DateTimeOffset(2026, 5, 23, 10, 15, 0, TimeSpan.Zero)
        );
        var document = session.CreateDocument(
            "plan",
            "Plan",
            "# Plan",
            "assistant",
            new DateTimeOffset(2026, 5, 23, 10, 16, 0, TimeSpan.Zero)
        );

        document.UpdateContent(
            "# Revised plan",
            new DateTimeOffset(2026, 5, 23, 10, 17, 0, TimeSpan.Zero)
        );

        Assert.Equal(2, document.Version);
        Assert.Equal("# Revised plan", document.ContentMarkdown);
    }

    private static BouwDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BouwDbContext>()
            .UseNpgsql("Host=localhost;Database=bouw_model_tests")
            .Options;

        return new BouwDbContext(options);
    }

    private static object? ConvertProperty(
        BouwDbContext context,
        Type entityType,
        string propertyName,
        object value
    ) =>
        context
            .Model.FindEntityType(entityType)
            ?.FindProperty(propertyName)
            ?.GetTypeMapping()
            .Converter?.ConvertToProvider(value);
}
