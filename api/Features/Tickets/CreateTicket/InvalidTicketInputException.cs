namespace Bouw.API.Features.Tickets.CreateTicket;

public sealed class InvalidTicketInputException : Exception
{
    public InvalidTicketInputException()
        : base("Ticket user input is required.") { }

    public InvalidTicketInputException(string message)
        : base(message) { }

    public InvalidTicketInputException(string message, Exception innerException)
        : base(message, innerException) { }
}
