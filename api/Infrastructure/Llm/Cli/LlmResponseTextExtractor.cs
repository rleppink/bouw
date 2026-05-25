using System.Text;
using System.Text.Json;

namespace Bouw.API.Infrastructure.Llm;

internal static class LlmResponseTextExtractor
{
    public delegate bool TryGetJsonTextEvent(JsonElement root, out string eventText);

    public static string ExtractNdjsonTextEvents(
        string standardOutput,
        TryGetJsonTextEvent tryGetTextEvent
    )
    {
        ArgumentNullException.ThrowIfNull(tryGetTextEvent);

        var text = new StringBuilder();
        using var reader = new StringReader(standardOutput);

        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            AppendTextEvent(line, text, tryGetTextEvent);
        }

        if (text.Length == 0)
        {
            throw new LlmBackendException("LLM backend returned no text events in NDJSON output.");
        }

        return text.ToString();
    }

    public static bool HasStringProperty(
        JsonElement element,
        string propertyName,
        string expectedValue
    )
    {
        return TryGetStringProperty(element, propertyName, out var value)
            && string.Equals(value, expectedValue, StringComparison.Ordinal);
    }

    public static bool TryGetStringProperty(
        JsonElement element,
        string propertyName,
        out string value
    )
    {
        value = string.Empty;

        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return true;
    }

    private static void AppendTextEvent(
        string line,
        StringBuilder text,
        TryGetJsonTextEvent tryGetTextEvent
    )
    {
        using var document = ParseLine(line);
        var root = document.RootElement;

        if (tryGetTextEvent(root, out var eventText))
        {
            text.Append(eventText);
        }
    }

    private static JsonDocument ParseLine(string line)
    {
        try
        {
            return JsonDocument.Parse(line);
        }
        catch (JsonException exception)
        {
            throw new LlmBackendException("LLM backend returned invalid NDJSON output.", exception);
        }
    }
}
