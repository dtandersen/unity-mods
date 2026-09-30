Feature: Chat mentions

  A public chat message that starts with the trigger is a question for the
  assistant. The trigger must be a whole word and must start the message.

  Scenario: A mention is recognised
    When the mention trigger is "!sage" and the message is "!sage how do I smelt?"
    Then the mention is recognised as "how do I smelt?"

  Scenario: A mention is case-insensitive and allows punctuation
    When the mention trigger is "!sage" and the message is "!SAGE: hello"
    Then the mention is recognised as "hello"

  Scenario: A bare mention has no question
    When the mention trigger is "!sage" and the message is "!sage"
    Then the message is not a mention

  Scenario: A longer word is not a mention
    When the mention trigger is "!sage" and the message is "!sageson hello"
    Then the message is not a mention

  Scenario: A mention must start the message
    When the mention trigger is "!sage" and the message is "hey !sage"
    Then the message is not a mention
