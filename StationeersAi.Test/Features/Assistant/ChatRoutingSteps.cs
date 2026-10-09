using Reqnroll;
using Shouldly;
using StationeersAi.Ai;

namespace StationeersAi.Test.Features.Assistant;

[Binding]
public sealed class ChatRoutingSteps
{
    private string _agentName = "Sage";
    private string _trigger = "@sage";
    private bool _respondToMentions = true;
    private bool _othersInGame;
    private ChatDecision _decision;

    [Given("the agent is named {string}")]
    public void GivenTheAgentIsNamed(string name)
    {
        _agentName = name;
    }

    [Given("the mention trigger is {string}")]
    public void GivenTheMentionTriggerIs(string trigger)
    {
        _trigger = trigger;
    }

    [Given("answering mentions is on")]
    public void GivenAnsweringMentionsIsOn()
    {
        _respondToMentions = true;
    }

    [Given("answering mentions is off")]
    public void GivenAnsweringMentionsIsOff()
    {
        _respondToMentions = false;
    }

    [Given("nobody else is in the game")]
    public void GivenNobodyElseIsInTheGame()
    {
        _othersInGame = false;
    }

    [Given("somebody else is in the game")]
    public void GivenSomebodyElseIsInTheGame()
    {
        _othersInGame = true;
    }

    [When("the line from {string} with text {string} is routed")]
    public void WhenTheLineFromWithTextIsRouted(string displayName, string text)
    {
        _decision = ChatRouting.Decide(
            new ChatLine(displayName, text),
            new ChatRoutingRules(_agentName, _trigger, _respondToMentions, _othersInGame));
    }

    [Then("the agent should answer {string}")]
    public void ThenTheAgentShouldAnswer(string expected)
    {
        _decision.ShouldAnswer.ShouldBeTrue();
        _decision.Prompt.ShouldBe(expected);
    }

    [Then("the agent should answer nothing")]
    public void ThenTheAgentShouldAnswerNothing()
    {
        _decision.ShouldAnswer.ShouldBeFalse();
        _decision.Prompt.ShouldBeNull();
    }

    [Then("the line was noted")]
    public void ThenTheLineWasNoted()
    {
        _decision.Considered.ShouldBeTrue();
    }

    [Then("the line was not noted")]
    public void ThenTheLineWasNotNoted()
    {
        _decision.Considered.ShouldBeFalse();
    }

    [Then("the line is from the agent")]
    public void ThenTheLineIsFromTheAgent()
    {
        _decision.FromAgent.ShouldBeTrue();
    }

    [Then("the line is not from the agent")]
    public void ThenTheLineIsNotFromTheAgent()
    {
        _decision.FromAgent.ShouldBeFalse();
    }
}
