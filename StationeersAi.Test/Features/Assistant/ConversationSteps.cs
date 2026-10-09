using System.Linq;
using System.Threading.Tasks;
using Reqnroll;
using Shouldly;
using Caev;
using StationeersAi.Test.Support;

namespace StationeersAi.Test.Features.Assistant;

[Binding]
public sealed class ConversationSteps
{
    private readonly ScenarioContext _context;

    public ConversationSteps(ScenarioContext context)
    {
        _context = context;
    }

    private WorldContext World => Hooks.GetWorld(_context);

    [Given("a conversation with a limit of {int} bytes")]
    public void GivenAConversationWithALimit(int limit)
    {
        World.Conversation = new Conversation(limit);
    }

    [Given("I add these conversation messages:")]
    public void WhenIAddTheseConversationMessages(Table table)
    {
        foreach (var row in table.Rows)
        {
            World.Conversation.Add(new ChatMessage(Role(row["Role"]), row["Content"]));
        }
    }

    [When("I clear the conversation")]
    public void WhenIClearTheConversation()
    {
        World.Conversation.Clear();
    }

    [When("I ask the assistant {string} with context")]
    public async Task WhenIAskTheAssistantWithContext(string prompt)
    {
        await World.Gateway.AskAsync(World.Turn(prompt, World.Conversation));
    }

    [When("I compact the conversation")]
    public async Task WhenICompactTheConversation()
    {
        await World.Gateway.CompactAsync(World.Conversation);
    }

    [Then("the conversation holds {int} message(s)")]
    public void ThenTheConversationSizeIs(int count)
    {
        World.Conversation.Messages.Count.ShouldBe(count);
    }

    [Then("the conversation contains the message {string}")]
    public void ThenTheConversationContains(string content)
    {
        World.Conversation.Messages.Any(message => message.Content == content).ShouldBeTrue(
            $"expected conversation to contain '{content}'");
    }

    [Then("the conversation does not contain the message {string}")]
    public void ThenTheConversationDoesNotContain(string content)
    {
        World.Conversation.Messages.Any(message => message.Content == content).ShouldBeFalse(
            $"did not expect conversation to contain '{content}'");
    }

    [Then("the AI model's last request contains a {string} message {string}")]
    public void ThenTheLastRequestContains(string role, string content)
    {
        var messages = World.Ai.Transcript.Last();
        messages.Any(message =>
                message.Role.ToString().ToLowerInvariant() == role.ToLowerInvariant() &&
                message.Content == content)
            .ShouldBeTrue($"expected a '{role}' message '{content}' in the last request");
    }

    private static ChatRole Role(string value)
    {
        switch (value.ToLowerInvariant())
        {
            case "assistant":
                return ChatRole.Assistant;
            case "system":
                return ChatRole.System;
            case "tool":
                return ChatRole.Tool;
            default:
                return ChatRole.User;
        }
    }
}
