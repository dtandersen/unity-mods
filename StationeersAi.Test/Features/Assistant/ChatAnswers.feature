Feature: Chat answers

  The agent answers in chat, whatever asked it: a window question, a console
  question, or a chat mention. One answer, once, in plain text, under the agent's
  name. The Sage window shows its own copy of the answer as well.

  Scenario: A window question is answered in chat
    Given the AI model will answer "Use **copper** and `cable`"
    And the window input is "what do I need?"
    When the user submits
    And the turn completes
    Then the chat shows:
      """
      Sage: Use copper and cable
      """

  Scenario: A console question is answered in chat
    Given the AI model will answer "42"
    When the console asks "what is the answer"
    And the turn completes
    Then the chat shows:
      """
      Sage: 42
      """

  Scenario: A window answer is shown in the window too
    Given the AI model will answer "42"
    When the console asks "what is the answer"
    And the turn completes
    Then the view shows:
      """
      Tester: what is the answer
      Sage: 42
      """
