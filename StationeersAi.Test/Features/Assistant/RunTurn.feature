Feature: Run turn

  One runner executes an agent turn and reports it through the turn's events:
  reasoning, tool calls and tool output as they happen, and the answer at the
  end. The caller owns the events object and subscribes to what it shows. The
  runner reports what never reaches the agent: the game-not-loaded notice and
  errors. The runner also assembles the turn from the config the game holds.

  Scenario: The answer arrives as an event
    Given the agent will answer "42"
    When the turn runs "what is the answer"
    Then the answered event is "42"

  Scenario: Tool calls arrive as events
    Given the agent will report the tool call "search_items(query=cable)"
    And the agent will answer "Cable needs 5 copper."
    When the turn runs "how do I make cable?"
    Then a tool call event is "search_items(query=cable)"

  Scenario: The turn carries the configured instructions and limit
    Given the turn instructions are "Look it up before answering"
    And the turn allows 3 tool rounds
    And the agent will answer "42"
    When the turn runs "what is the answer"
    Then the turn sent the instructions "Look it up before answering" and 3 tool rounds

  Scenario: Missing game data arrives as an answer
    Given game data is not loaded
    When the turn runs "hello"
    Then the answered event is "game data is not loaded yet."
