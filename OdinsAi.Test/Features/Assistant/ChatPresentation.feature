Feature: Chat presentation

  The assistant's turn is presented through an IChatSink. The player's question is
  echoed first (so it stays in the log), then the answer. When the assistant's
  activity is not shown, a Thinking... status is posted after 10 seconds of quiet.

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

  Scenario: A delayed status appears when the model is slow and thinking is off
    Given the AI model will answer "42"
    And the AI model will wait
    And thinking display is off
    When I start asking the assistant in chat "hello"
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

  Scenario: No status while thinking display is on
    Given the AI model will answer "42"
    And the AI model will wait
    And thinking display is on
    When I start asking the assistant in chat "hello"
    And I advance the clock by 10 seconds
    When I release the AI model
    Then the chat shows:
      """
      Tester: hello
      Odin: 42
      """