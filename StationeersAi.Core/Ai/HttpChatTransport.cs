using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StationeersAi.Ai;

/// <summary>
/// Real HTTP transport using <see cref="HttpClient"/>. Usable outside Unity, so
/// the OpenAI client can be exercised over a real socket in tests.
/// </summary>
public sealed class HttpChatTransport : IChatTransport
{
    private readonly HttpClient _client;

    public HttpChatTransport(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<HttpReply> PostAsync(HttpRequest request, CancellationToken cancellationToken = default)
    {
        using (var content = new StringContent(request.Body, Encoding.UTF8, "application/json"))
        using (var message = new HttpRequestMessage(HttpMethod.Post, request.Url) { Content = content })
        {
            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    message.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            using (var response = await _client.SendAsync(message, cancellationToken).ConfigureAwait(false))
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return new HttpReply((int)response.StatusCode, body);
            }
        }
    }
}
