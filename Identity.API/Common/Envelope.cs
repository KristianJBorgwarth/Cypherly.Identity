using System.Text.Json.Serialization;

namespace Identity.API.Common;

/// <summary>
/// The responsibility for the envelope is to wrap any result to create a consistent resultset to the clients.
/// </summary>
public class Envelope : Envelope<string>
{
    [JsonConstructor]
    protected internal Envelope(string errorMessage) : base(null, errorMessage)
    {
    }

    public static Envelope<T> Ok<T>(T result)
    {
        return new Envelope<T>(result, null);
    }

    public static Envelope Ok()
    {
        return new Envelope(null);
    }

    public static Envelope Error(string errormessage)
    {
        return new Envelope(errormessage);
    }
}

public class Envelope<T>
{
    public T Result { get; init; } = default!;
    public string ErrorMessage { get; init; } = string.Empty;
    public DateTime TimeGenerated { get; set; } = DateTime.UtcNow;

    [JsonConstructor]
    protected internal Envelope(T result, string errorMessage)
    {
        Result = result;
        ErrorMessage = errorMessage;
        TimeGenerated = DateTime.UtcNow;
    }
}