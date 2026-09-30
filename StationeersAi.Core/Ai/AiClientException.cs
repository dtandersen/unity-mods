using System;

namespace StationeersAi.Ai
{
    /// <summary>Raised when a model request fails: transport, HTTP status, or malformed response.</summary>
    public sealed class AiClientException : Exception
    {
        public AiClientException(string message)
            : base(message)
        {
        }

        public AiClientException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
