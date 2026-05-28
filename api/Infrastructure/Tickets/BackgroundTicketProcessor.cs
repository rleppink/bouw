using System.Threading.Channels;
using Bouw.API.Persistence;
using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bouw.API.Infrastructure.Tickets;

public sealed class BackgroundTicketProcessor : BackgroundService, ITicketProcessor
{
    private static readonly Action<ILogger, Guid, Exception?> CouldNotProcessTicket =
        LoggerMessage.Define<Guid>(
            LogLevel.Error,
            new EventId(1, nameof(CouldNotProcessTicket)),
            "Could not process ticket {TicketId}."
        );

    private static readonly TimeSpan ResponseDelay = TimeSpan.FromSeconds(5);

    private readonly Channel<Guid> queue = Channel.CreateUnbounded<Guid>();
    private readonly IServiceScopeFactory scopeFactory;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<BackgroundTicketProcessor> logger;

    public BackgroundTicketProcessor(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<BackgroundTicketProcessor> logger
    )
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);

        this.scopeFactory = scopeFactory;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public void Enqueue(Guid ticketId)
    {
        if (!this.queue.Writer.TryWrite(ticketId))
        {
            throw new InvalidOperationException($"Could not enqueue ticket '{ticketId}'.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (
            var ticketId in this.queue.Reader.ReadAllAsync(stoppingToken).ConfigureAwait(false)
        )
        {
            try
            {
                await this.ProcessAsync(ticketId, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                CouldNotProcessTicket(this.logger, ticketId, ex);
            }
            catch (DbUpdateException ex)
            {
                CouldNotProcessTicket(this.logger, ticketId, ex);
            }
        }
    }

    private async Task ProcessAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        using var scope = this.scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BouwDbContext>();

        var ticket = await db
            .Tickets.AsTracking()
            .SingleOrDefaultAsync(ticket => ticket.Id == ticketId, cancellationToken)
            .ConfigureAwait(false);

        if (ticket is null || ticket.Status == TicketStatus.Completed)
        {
            return;
        }

        await Task.Delay(ResponseDelay, cancellationToken).ConfigureAwait(false);

        ticket.Complete(Reverse(ticket.UserInput), this.timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string Reverse(string value)
    {
        var characters = value.ToCharArray();
        Array.Reverse(characters);
        return new string(characters);
    }
}
