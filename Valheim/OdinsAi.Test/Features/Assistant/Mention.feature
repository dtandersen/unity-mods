Feature: Chat mentions

  A public chat message that starts with the trigger is a question for the
  assistant. The trigger must be a whole word and must start the message.

  Scenario: A mention is recognised
    When the mention trigger is "!odin" and the message is "!odin how do I cook?"
    Then the mention is recognised as "how do I cook?"

  Scenario: A mention is case-insensitive and allows punctuation
    When the mention trigger is "!odin" and the message is "!ODIN: hello"
    Then the mention is recognised as "hello"

  Scenario: A bare mention has no question
    When the mention trigger is "!odin" and the message is "!odin"
    Then the message is not a mention

  Scenario: A longer word is not a mention
    When the mention trigger is "!odin" and the message is "!odinson hello"
    Then the message is not a mention

  Scenario: A mention must start the message
    When the mention trigger is "!odin" and the message is "hey !odin"
    Then the message is not a mention