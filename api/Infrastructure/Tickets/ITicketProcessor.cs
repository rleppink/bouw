namespace Bouw.API.Infrastructure.Tickets;

public interface ITicketProcessor
{
    void Enqueue(Guid ticketId);
}
