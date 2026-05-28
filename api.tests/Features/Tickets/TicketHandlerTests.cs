using Bouw.API.Infrastructure.Tickets;
using Bouw.API.Persistence;
using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using CreateTicket = Bouw.API.Features.Tickets.CreateTicket;
using GetTicket = Bouw.API.Features.Tickets.GetTicket;
using GetTickets = Bouw.API.Features.Tickets.GetTickets;

namespace Bouw.API.Tests.Features.Tickets;

public sealed class TicketHandlerTests
{
    [Fact]
    public async Task CreateTicketPersistsPendingTicketAndEnqueuesProcessing()
    {
        await using var context = CreateContext();
        var now = new DateTimeOffset(2026, 5, 25, 8, 30, 0, TimeSpan.Zero);
        var processor = new RecordingTicketProcessor();
        var handler = new CreateTicket.Handler(context, processor, new FixedTimeProvider(now));

        var ticket = await handler.HandleAsync(
            new CreateTicket.CreateTicketRequest("Build me a thing"),
            CancellationToken.None
        );

        var persisted = await context.Tickets.SingleAsync(CancellationToken.None);
        Assert.Equal("Build me a thing", ticket.Title);
        Assert.Equal("Build me a thing", persisted.UserInput);
        Assert.Equal("pending", ticket.Status);
        Assert.Equal(string.Empty, persisted.LlmResponse);
        Assert.Equal(now, persisted.CreatedAt);
        Assert.Equal(now, persisted.UpdatedAt);
        Assert.Equal(ticket.Id, processor.EnqueuedTicketId);
    }

    [Fact]
    public void CompletingTicketStoresResponseAndUpdatedTimestamp()
    {
        var createdAt = new DateTimeOffset(2026, 5, 25, 8, 30, 0, TimeSpan.Zero);
        var completedAt = new DateTimeOffset(2026, 5, 25, 8, 31, 0, TimeSpan.Zero);
        var ticket = new Ticket("Build me a thing", createdAt);

        ticket.Complete("gniht a em dliuB", completedAt);

        Assert.Equal(TicketStatus.Completed, ticket.Status);
        Assert.Equal("gniht a em dliuB", ticket.LlmResponse);
        Assert.Equal(completedAt, ticket.UpdatedAt);
    }

    [Fact]
    public async Task CreateTicketDerivesTitleFromFirstNonEmptyLineAndTruncates()
    {
        await using var context = CreateContext();
        var handler = new CreateTicket.Handler(
            context,
            new RecordingTicketProcessor(),
            new FixedTimeProvider(DateTimeOffset.UtcNow)
        );
        var firstLine = new string('a', 81);

        var ticket = await handler.HandleAsync(
            new CreateTicket.CreateTicketRequest($"\n  {firstLine}  \nsecond"),
            CancellationToken.None
        );

        Assert.Equal(new string('a', 80), ticket.Title);
    }

    [Fact]
    public async Task CreateTicketRejectsWhitespaceOnlyInput()
    {
        await using var context = CreateContext();
        var handler = new CreateTicket.Handler(
            context,
            new RecordingTicketProcessor(),
            new FixedTimeProvider(DateTimeOffset.UtcNow)
        );

        await Assert.ThrowsAsync<CreateTicket.InvalidTicketInputException>(() =>
            handler.HandleAsync(new CreateTicket.CreateTicketRequest("   "), CancellationToken.None)
        );
    }

    [Fact]
    public async Task GetTicketsReturnsNewestFirst()
    {
        await using var context = CreateContext();
        var older = new CreateTicket.Handler(
            context,
            new RecordingTicketProcessor(),
            new FixedTimeProvider(new DateTimeOffset(2026, 5, 25, 8, 0, 0, TimeSpan.Zero))
        );
        var newer = new CreateTicket.Handler(
            context,
            new RecordingTicketProcessor(),
            new FixedTimeProvider(new DateTimeOffset(2026, 5, 25, 9, 0, 0, TimeSpan.Zero))
        );
        await older.HandleAsync(
            new CreateTicket.CreateTicketRequest("one"),
            CancellationToken.None
        );
        await newer.HandleAsync(
            new CreateTicket.CreateTicketRequest("two"),
            CancellationToken.None
        );

        var tickets = await new GetTickets.Handler(context).HandleAsync(CancellationToken.None);

        Assert.Equal(["two", "one"], tickets.Select(ticket => ticket.UserInput));
    }

    [Fact]
    public async Task GetTicketReturnsNullForUnknownId()
    {
        await using var context = CreateContext();
        var ticket = await new GetTicket.Handler(context).HandleAsync(
            Guid.NewGuid(),
            CancellationToken.None
        );

        Assert.Null(ticket);
    }

    private static BouwDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BouwDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new BouwDbContext(options);
    }

    private sealed class RecordingTicketProcessor : ITicketProcessor
    {
        public Guid EnqueuedTicketId { get; private set; }

        public void Enqueue(Guid ticketId)
        {
            this.EnqueuedTicketId = ticketId;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
