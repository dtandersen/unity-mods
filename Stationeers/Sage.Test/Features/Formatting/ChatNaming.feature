Feature: Assistant chat name

  The assistant's display name is configurable and defaults to "Sage". Every chat
  line the assistant posts is prefixed with that name.

  Scenario: The default name is used
    When the assistant name is ""
    Then the assistant is called "Sage"
    And a chat line for "hello" is "Sage: hello"

  Scenario: A whitespace name falls back to the default
    When the assistant name is "   "
    Then the assistant is called "Sage"

  Scenario: The configured name is used
    When the assistant name is "Ada"
    Then the assistant is called "Ada"
    And a chat line for "hello" is "Ada: hello"

  Scenario: The configured name is trimmed
    When the assistant name is "  Ada  "
    Then the assistant is called "Ada"
    And a chat line for "hello" is "Ada: hello"
