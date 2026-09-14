using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OdinsAi.Ai
{
    /// <summary>A raw HTTP request to the model provider.</summary>
    public sealed record HttpRequest(
        string Url,
        IReadOnlyDictionary<string, string> Headers,
        string Body);

    /// <summary>The raw HTTP reply from the model provider.</summary>
    public sealed record HttpReply(int StatusCode, string Body);

    /// <summary>
    /// The raw HTTP boundary. The Unity plugin implements this with
    /// <c>UnityWebRequest</c>; tests use a real socket or a fake.
    /// </summary>
    public interface IChatTransport
    {
        Task<HttpReply> PostAsync(HttpRequest request, CancellationToken cancellationToken = default);
    }
}