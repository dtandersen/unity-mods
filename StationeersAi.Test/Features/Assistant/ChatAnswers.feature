Feature: Chat answers

  The agent answers in chat, whatever asked it: a window question, a console
  question, or a chat mention. One answer, once, in plain text, and posted under
  the agent's name so the game shows who speaks. The Sage window shows its own
  copy of the answer too.

  One question at a time, on every surface: while a turn runs, the next is
  refused. The window, the console, and chat share that one gate.

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

  Scenario: A chat mention is answered in chat
    Given the AI model will answer "Cable Coil"
    When the player mentions "!sage how do I wire?"
    Then the chat shows:
      """
      Sage: Cable Coil
      """

  Scenario: A chat answer is broadcast as plain text
    Given the AI model will answer "Use **copper** and `cable`"
    When the player mentions "!sage what do I need?"
    Then the chat shows:
      """
      Sage: Use copper and cable
      """

  Scenario: A second question is refused while one runs
    Given the AI model will answer "First answer"
    And the AI model will wait
    When I start a mention "!sage one"
    And the player mentions "!sage two"
    When I release the AI model
    Then the chat shows:
      """
      Sage: First answer
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
