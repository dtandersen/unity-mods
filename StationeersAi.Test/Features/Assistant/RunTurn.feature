Feature: Run turn

  One runner executes an assistant turn and reports it through events:
  reasoning, tool calls and tool output as they happen, and the answer at
  the end. Callers subscribe to what they show.

  Scenario: The answer arrives as an event
    Given the AI model will answer "42"
    When the turn runs "what is the answer"
    Then the answered event is "42"

  Scenario: Tool calls arrive as events
    Given the AI model will request the tool "search_items" with query "cable"
    And the AI model will answer by repeating the last tool result
    When the turn runs "how do I make cable?"
    Then a tool call event is "search_items(query=cable)"

  Scenario: Missing game data arrives as an answer
    Given game data is not loaded
    When the turn runs "hello"
    Then the answered event is "game data is not loaded yet."
