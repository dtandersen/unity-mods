Feature: Run turn

  One runner executes an assistant turn and reports it through the turn's
  events: reasoning, tool calls and tool output as they happen, and the answer
  at the end. The caller owns the events object and subscribes to what it shows.
  The runner reports what never reaches the assistant: the game-not-loaded
  notice and errors.

  Scenario: The answer arrives as an event
    Given the assistant will answer "42"
    When the turn runs "what is the answer"
    Then the answered event is "42"

  Scenario: Tool calls arrive as events
    Given the assistant will report the tool call "search_items(query=cable)"
    And the assistant will answer "Cable needs 5 copper."
    When the turn runs "how do I make cable?"
    Then a tool call event is "search_items(query=cable)"

  Scenario: Missing game data arrives as an answer
    Given game data is not loaded
    When the turn runs "hello"
    Then the answered event is "game data is not loaded yet."
