using System;

namespace StationeersAi.Ai;

/// <summary>Raised when a model request fails: transport, HTTP status, or malformed response.</summary>
public sealed class AiClientException : Exception
{
    public AiClientException(string message)
        : base(message)
    {
    }

    public AiClientException(string message, int? statusCode, Exception inner)
        : base(message, inner)
    {
        StatusCode = statusCode;
    }

    /// <summary>The HTTP status when the service answered, or null when it never did.</summary>
    public int? StatusCode { get; }
}
