Feature: Tool activity reporting

  The assistant reports tool calls (thinking) and tool output through the turn's
  events. The plugin subscribes to these for its ShowThinking and ShowToolResults
  options.

  Scenario: Tool activity is reported
    Given my inventory contains:
      | Item   | Quantity | Slot       |
      | Welder | 85%      | Right hand |
    And the AI model will request the tool "search_inventory" with query "welder"
    And the AI model will answer "You have a welder."
    When I ask the assistant "what am I holding?" reporting events
    Then the tool calls reported "search_inventory(query=welder)"
    And the tool output reported:
      """
      Name,Quantity,Slot,Hand
      Welder,85%,Right hand,Right
      """

  Scenario: The model's reasoning is reported as thinking
    Given the AI model reasons "weigh the options" and answers "Go north"
    When I ask the assistant "which way?" reporting events
    Then the thinking reported "weigh the options"
    And the assistant replies "Go north"
