Feature: Chat presentation

  The assistant's turn is presented through an IChatSink. The player's question is
  echoed first (so it stays in the log), then a Thinking... status, then the answer.

  Scenario: The question is echoed and the answer posted
    Given the AI model will answer "42"
    When I ask the assistant in chat "what is the answer"
    Then the chat shows:
      """
      Tester: what is the answer
      Odin: Thinking...
      Odin: 42
      """

  Scenario: The configured assistant name is used
    Given the AI model will answer "42"
    And the assistant is named "Munin"
    When I ask the assistant in chat "hello"
    Then the chat shows:
      """
      Tester: hello
      Munin: Thinking...
      Munin: 42
      """

  Scenario: A missing player name falls back to You
    Given the AI model will answer "42"
    And my player name is ""
    When I ask the assistant in chat "hello"
    Then the chat shows:
      """
      You: hello
      Odin: Thinking...
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