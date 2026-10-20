Feature: Mention responder

  The responder answers one public mention at a time and ignores further mentions
  while it is busy. The answer is broadcast as plain text under the assistant's
  name.

  Scenario: A mention is answered in chat
    Given the AI model will answer "Boar stew"
    When the player mentions "!odin how do I cook?"
    Then the chat shows:
      """
      Odin: Boar stew
      """

  Scenario: The answer is broadcast as plain text
    Given the AI model will answer "Use **carrots** and `turnips`"
    When the player mentions "!odin how do I tame a boar?"
    Then the chat shows:
      """
      Odin: Use carrots and turnips
      """

  Scenario: Further mentions are ignored while busy
    Given the AI model will answer "First answer"
    And the AI model will wait
    When I start a mention "!odin one"
    And the player mentions "!odin two"
    When I release the AI model
    Then the chat shows:
      """
      Odin: First answer
      """