Feature: Chat presentation

  The assistant's turn is presented through an IChatSink. The player's question is
  echoed first (so it stays in the log), then the answer. The presenter decides
  which of the assistant's activity to show, and posts a Thinking... placeholder
  when hidden activity is followed by 10 quiet seconds.

  Scenario: The question is echoed and the answer posted
    Given the AI model will answer "42"
    When I ask the assistant in chat "what is the answer"
    Then the chat shows:
      """
      Tester: what is the answer
      Odin: 42
      """

  Scenario: The configured assistant name is used
    Given the AI model will answer "42"
    And the assistant is named "Munin"
    When I ask the assistant in chat "hello"
    Then the chat shows:
      """
      Tester: hello
      Munin: 42
      """

  Scenario: A missing player name falls back to You
    Given the AI model will answer "42"
    And my player name is ""
    When I ask the assistant in chat "hello"
    Then the chat shows:
      """
      You: hello
      Odin: 42
      """

  Scenario: The question is still echoed when game data is not loaded
    Given game data is not loaded
    When I ask the assistant in chat "hello"
    Then the chat shows:
      """
      Tester: hello
      Odin: game data is not loaded yet.
      """

  Scenario: Reasoning is shown instead of a placeholder when thinking is on
    Given the AI model reasons "weigh the options" and answers "Go north"
    And thinking display is on
    When I ask the assistant in chat "which way"
    Then the chat shows:
      """
      Tester: which way
      Odin: weigh the options
      Odin: Go north
      """

  Scenario: Hidden activity gets a Thinking... placeholder after 10 quiet seconds
    Given the AI model will answer "42"
    And the AI model will wait
    And thinking display is off
    When I start asking the assistant in chat "hello"
    And the assistant reports a tool call "get_location()"
    And I advance the clock by 10 seconds
    Then the chat shows:
      """
      Tester: hello
      Odin: Thinking...
      """
    When I release the AI model
    Then the chat shows:
      """
      Tester: hello
      Odin: Thinking...
      Odin: 42
      """