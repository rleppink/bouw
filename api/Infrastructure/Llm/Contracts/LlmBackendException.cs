namespace Bouw.API.Infrastructure.Llm;

public sealed class LlmBackendException : Exception
{
    public LlmBackendException() { }

    public LlmBackendException(string message)
        : base(message) { }

    public LlmBackendException(string message, Exception innerException)
        : base(message, innerException) { }
}
