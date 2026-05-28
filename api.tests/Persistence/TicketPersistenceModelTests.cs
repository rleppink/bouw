using Bouw.API.Persistence;
using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bouw.API.Tests.Persistence;

public sealed class TicketPersistenceModelTests
{
    [Fact]
    public void ModelContainsTicketsTableAndColumns()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(Ticket));

        Assert.Equal("tickets", entity?.GetTableName());
        Assert.Equal("uuid", entity?.FindProperty("Id")?.GetColumnType());
        Assert.Equal("character varying(80)", entity?.FindProperty("Title")?.GetColumnType());
        Assert.Equal("text", entity?.FindProperty("UserInput")?.GetColumnType());
        Assert.Equal("character varying(40)", entity?.FindProperty("Status")?.GetColumnType());
        Assert.Equal("text", entity?.FindProperty("LlmResponse")?.GetColumnType());
        Assert.Equal(
            "timestamp with time zone",
            entity?.FindProperty("CreatedAt")?.GetColumnType()
        );
        Assert.Equal(
            "timestamp with time zone",
            entity?.FindProperty("UpdatedAt")?.GetColumnType()
        );
    }

    [Fact]
    public void TicketStatusUsesDocumentVocabulary()
    {
        using var context = CreateContext();

        Assert.Equal(
            "completed",
            context
                .Model.FindEntityType(typeof(Ticket))
                ?.FindProperty("Status")
                ?.GetTypeMapping()
                .Converter?.ConvertToProvider(TicketStatus.Completed)
        );
        Assert.Equal(
            "pending",
            context
                .Model.FindEntityType(typeof(Ticket))
                ?.FindProperty("Status")
                ?.GetTypeMapping()
                .Converter?.ConvertToProvider(TicketStatus.Pending)
        );
    }

    private static BouwDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BouwDbContext>()
            .UseNpgsql("Host=localhost;Database=bouw_model_tests")
            .Options;

        return new BouwDbContext(options);
    }
}
