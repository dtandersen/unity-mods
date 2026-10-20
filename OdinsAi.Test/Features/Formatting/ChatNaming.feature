Feature: Assistant chat name

  The assistant's display name is configurable and defaults to "Odin". Every chat
  line the assistant posts is prefixed with that name.

  Scenario: The default name is used
    When the assistant name is ""
    Then the assistant is called "Odin"
    And a chat line for "hello" is "Odin: hello"

  Scenario: A whitespace name falls back to the default
    When the assistant name is "   "
    Then the assistant is called "Odin"

  Scenario: The configured name is used
    When the assistant name is "Munin"
    Then the assistant is called "Munin"
    And a chat line for "hello" is "Munin: hello"

  Scenario: The configured name is trimmed
    When the assistant name is "  Freya  "
    Then the assistant is called "Freya"
    And a chat line for "hello" is "Freya: hello"