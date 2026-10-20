using System;
using System.Threading.Tasks;
using OdinsAi.Ai;
using OdinsAi.Ai.OpenAi;
using OdinsAi.Tool;
using Shouldly;
using TUnit.Core;

namespace OdinsAi.Test.Integration
{
    /// <summary>
    /// Opt-in tests that hit a real OpenAI-compatible endpoint. Excluded from normal
    /// runs by <see cref="ExplicitAttribute"/>. Run them with:
    /// <code>
    /// dotnet run --project OdinsAi.Test -- --treenode-filter "/*/*/*/*[Category=Integration]"
    /// </code>
    /// Credentials come from ODINSAI_ENDPOINT/ODINSAI_API_KEY/ODINSAI_MODEL or a
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
                new HttpChatTransport(),
                new OpenAiOptions(settings.ChatCompletionsUrl, settings.ApiKey, settings.Model));

            var completion = await client.CompleteAsync(
                new[] { new ChatMessage(ChatRole.User, "Reply with the single word: pong") },
                Array.Empty<ToolDefinition>());

            completion.Content.ShouldNotBeNullOrWhiteSpace();
            completion.Content.ToLowerInvariant().ShouldContain("pong");
        }
    }
}