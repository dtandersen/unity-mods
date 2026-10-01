using System.Net.Http;
using System.Threading.Tasks;
using Caev;
using Caev.Tools;
using Shouldly;
using TUnit.Core;
using Caev.OpenAi;

namespace StationeersAi.Test.Integration;

/// <summary>
/// Opt-in tests that hit a real OpenAI-compatible endpoint. Excluded from normal
/// runs by <see cref="ExplicitAttribute"/>. Run them with:
/// <code>
/// dotnet run --project StationeersAi.Test -- --treenode-filter "/*/*/*/*[Category=Integration]"
/// </code>
/// Credentials come from SAGEAI_ENDPOINT/SAGEAI_API_KEY/SAGEAI_MODEL or a
/// git-ignored integration.settings.json beside the test assembly.
/// </summary>
public class LiveModelTests
{
    [Test]
    [Category("Integration")]
    [Explicit]
    public async Task The_live_model_answers_a_prompt()
    {
        var settings = IntegrationSettings.Load();
        if (settings == null)
        {
            Skip.Test("Integration settings not configured (env vars or integration.settings.json).");
            return;
        }

        var client = new OpenAiChatClient(
            new HttpChatTransport(new HttpClient()),
            new OpenAiOptions(settings.ChatCompletionsUrl, settings.ApiKey, settings.Model, null));

        var completion = await client.CompleteAsync(
            new[] { new ChatMessage(ChatRole.User, "Reply with the single word: pong") },
            System.Array.Empty<ToolDefinition>());

        completion.Content.ShouldNotBeNullOrWhiteSpace();
        completion.Content.ToLowerInvariant().ShouldContain("pong");
    }
}
