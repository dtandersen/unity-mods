using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Caev;
using Caev.Tools;
using Caev.OpenAi;

namespace StationeersAi.Test;

/// <summary>Retry after retryable HTTP failures: 408/429/5xx, transport included.</summary>
public class OpenAiRetryTests
{
    private const string OkBody = "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"ok\"}}]}";

    private sealed class ScriptedTransport : IChatTransport
    {
        private readonly Queue<object> _steps = new Queue<object>();

        public int Calls { get; private set; }

        public ScriptedTransport(params object[] steps)
        {
            foreach (var step in steps)
            {
                _steps.Enqueue(step);
            }
        }

        public Task<HttpReply> PostAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            Calls++;
            var step = _steps.Dequeue();
            if (step is Exception ex)
            {
                return Task.FromException<HttpReply>(ex);
            }

            return Task.FromResult((HttpReply)step);
        }
    }

    private static OpenAiChatClient Client(ScriptedTransport transport)
    {
        return new OpenAiChatClient(transport, new OpenAiOptions("http://localhost/v1/chat/completions", "key", "model", null));
    }

    private static IReadOnlyList<ChatMessage> Messages()
    {
        return new[] { new ChatMessage(ChatRole.User, "Hi") };
    }

    [Test]
    public async Task Retries_503_then_succeeds()
    {
        var transport = new ScriptedTransport(
            new HttpReply(503, "busy"),
            new HttpReply(503, "busy"),
            new HttpReply(200, OkBody));

        var completion = await Client(transport).CompleteAsync(Messages(), Array.Empty<ToolDefinition>());

        completion.Content.ShouldBe("ok");
        transport.Calls.ShouldBe(3);
    }

    [Test]
    public async Task Gives_up_after_three_retries()
    {
        var transport = new ScriptedTransport(
            new HttpReply(503, "busy"),
            new HttpReply(503, "busy"),
            new HttpReply(503, "busy"),
            new HttpReply(503, "busy"),
            new HttpReply(200, OkBody));

        var ex = await Should.ThrowAsync<AiClientException>(() =>
            Client(transport).CompleteAsync(Messages(), Array.Empty<ToolDefinition>()));

        ex.Message.ShouldContain("503");
        ex.Message.ShouldContain("after 4 attempts");
        transport.Calls.ShouldBe(4);
    }

    [Test]
    public async Task Retries_transport_failures_carrying_a_status()
    {
        var transport = new ScriptedTransport(
            new AiClientException("Request failed: HTTP 503", 503, null),
            new HttpReply(200, OkBody));

        var completion = await Client(transport).CompleteAsync(Messages(), Array.Empty<ToolDefinition>());

        completion.Content.ShouldBe("ok");
        transport.Calls.ShouldBe(2);
    }

    [Test]
    public async Task Does_not_retry_401()
    {
        var transport = new ScriptedTransport(new HttpReply(401, "nope"));

        await Should.ThrowAsync<AiClientException>(() =>
            Client(transport).CompleteAsync(Messages(), Array.Empty<ToolDefinition>()));

        transport.Calls.ShouldBe(1);
    }

    [Test]
    public async Task Does_not_retry_when_the_service_is_unreachable()
    {
        var transport = new ScriptedTransport(
            new AiClientException("Request failed: connection refused", null, new InvalidOperationException("down")));

        await Should.ThrowAsync<AiClientException>(() =>
            Client(transport).CompleteAsync(Messages(), Array.Empty<ToolDefinition>()));

        transport.Calls.ShouldBe(1);
    }
}
