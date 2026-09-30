Feature: Mention responder

  The responder answers one public mention at a time and ignores further mentions
  while it is busy. The answer is broadcast as plain text under the assistant's
  name.

  Scenario: A mention is answered in chat
    Given the AI model will answer "Cable Coil"
    When the player mentions "!sage how do I wire?"
    Then the chat shows:
      """
      Sage: Cable Coil
      """

  Scenario: The answer is broadcast as plain text
    Given the AI model will answer "Use **copper** and `cable`"
    When the player mentions "!sage what do I need?"
    Then the chat shows:
      """
      Sage: Use copper and cable
      """

  Scenario: Further mentions are ignored while busy
    Given the AI model will answer "First answer"
    And the AI model will wait
    When I start a mention "!sage one"
    And the player mentions "!sage two"
    When I release the AI model
    Then the chat shows:
      """
      Sage: First answer
      """
