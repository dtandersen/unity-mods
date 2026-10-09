Feature: Chat routing

  A chat line the game displays is either a question for the agent or not. The
  agent's own lines are never a question: they come back through the display path
  like anyone else's. Alone on the server every line is a question; with another
  player, only a line that starts with the trigger is.

  Routing also says whether a line is worth noting in the log: the agent's own
  lines, an empty line, and any line while answering is off are not, so a switched-off
  mod does not copy all of chat into the log.

  Routing does not decide what the Sage window shows. The window takes its lines
  from the gateway's channel, which the presenter listens on; the mod still hides
  the agent's own chat lines with one early return, so those do not reach the window
  from chat.

  The mod decides who is in the game. When it cannot read the player count it says
  somebody is, so a plain line goes unanswered; that guess stays in the mod and is
  not routed here.

  Background:
    Given the agent is named "Sage"
    And the mention trigger is "@sage"
    And answering mentions is on

  Scenario: Alone on the server, a plain line is a question
    Given nobody else is in the game
    When the line from "onebit" with text "how do I smelt?" is routed
    Then the agent should answer "how do I smelt?"
    And the line is not from the agent
    And the line was noted

  Scenario: With another player, a plain line is nobody's question
    Given somebody else is in the game
    When the line from "onebit" with text "how do I smelt?" is routed
    Then the agent should answer nothing
    And the line was noted

  Scenario: A mention is a question whoever is in the game
    Given somebody else is in the game
    When the line from "onebit" with text "@sage how do I smelt?" is routed
    Then the agent should answer "how do I smelt?"

  Scenario: Alone, a bare trigger is a question as written
    Given nobody else is in the game
    When the line from "onebit" with text "@sage" is routed
    Then the agent should answer "@sage"

  Scenario: The agent's own line is never a question
    Given nobody else is in the game
    When the line from "Sage" with text "Cable Coil needs copper." is routed
    Then the agent should answer nothing
    And the line is from the agent
    And the line was not noted

  Scenario: The agent's own line is recognised whatever its case
    Given somebody else is in the game
    When the line from "SAGE" with text "@sage hello" is routed
    Then the agent should answer nothing
    And the line is from the agent

  Scenario: Nothing is answered when answering is off
    Given nobody else is in the game
    And answering mentions is off
    When the line from "onebit" with text "@sage how do I smelt?" is routed
    Then the agent should answer nothing
    And the line was not noted

  Scenario: A blank line is no question
    Given nobody else is in the game
    When the line from "onebit" with text "   " is routed
    Then the agent should answer nothing
    And the line was not noted

  Scenario: A line with no name is not the agent
    Given nobody else is in the game
    When the line from "" with text "how do I smelt?" is routed
    Then the agent should answer "how do I smelt?"
    And the line is not from the agent
