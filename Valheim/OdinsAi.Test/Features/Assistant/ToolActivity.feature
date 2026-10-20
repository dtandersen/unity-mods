Feature: Tool activity reporting

  The assistant reports tool calls (thinking) and tool output through optional
  callbacks. The plugin uses these for its ShowThinking and ShowToolResults
  options.

  Scenario: Tool activity is reported
    Given my inventory contains:
      | Item   | Amount | Type     |
      | Turnip | 8      | material |
    And the AI model will request the tool "search_inventory" with query "turnip"
    And the AI model will answer "You have 8 turnips."
    When I ask the assistant "how many turnips do I have?" reporting activity
    Then the tool calls reported "search_inventory(query=turnip)"
    And the tool output reported:
      """
      Name,Amount
      Turnip,8
      """

  Scenario: The model's reasoning is reported as thinking
    Given the AI model reasons "weigh the options" and answers "Go north"
    When I ask the assistant "which way?" reporting activity
    Then the thinking reported "weigh the options"
    And the assistant replies "Go north"