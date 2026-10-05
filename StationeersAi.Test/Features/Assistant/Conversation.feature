Feature: Conversation context

  The assistant keeps a conversation history between questions so the user can
  refer back to earlier turns. The history has a byte budget and can be
  compacted into a summary. Each message costs its text in bytes plus 32
  bytes of framing, so the budget fits few large turns or many small ones.

  Scenario: History is sent with the next question
    Given the AI model will answer "First answer."
    And the AI model will answer "Second answer."
    When I ask the assistant "first question" with context
    And I ask the assistant "second question" with context
    Then the AI model's last request contains a "user" message "first question"
    And the AI model's last request contains a "assistant" message "First answer."
    And the AI model's last request contains a "user" message "second question"

  Scenario: Old turns drop whole once the budget runs out
    Given a conversation with a limit of 140 bytes
    And I add these conversation messages:
      | Role      | Content |
      | user      | m1      |
      | assistant | m2      |
      | user      | m3      |
      | assistant | m4      |
      | user      | m5      |
      | assistant | m6      |
    Then the conversation holds 4 messages
    And the conversation contains the message "m3"
    And the conversation does not contain the message "m2"

  Scenario: A lone new turn survives even over budget
    Given a conversation with a limit of 60 bytes
    And I add these conversation messages:
      | Role      | Content |
      | user      | one     |
      | assistant | two     |
      | user      | three   |
    Then the conversation holds 1 message
    And the conversation contains the message "three"

  Scenario: Small messages share the budget by bytes, not count
    Given a conversation with a limit of 200 bytes
    And I add these conversation messages:
      | Role      | Content |
      | user      | m1      |
      | assistant | m2      |
      | user      | m3      |
      | assistant | m4      |
      | user      | m5      |
    Then the conversation holds 5 messages

  Scenario: Clearing empties the conversation
    Given a conversation with a limit of 100 bytes
    And I add these conversation messages:
      | Role | Content |
      | user | hello   |
    When I clear the conversation
    Then the conversation holds 0 messages

  Scenario: Compacting replaces the context with a summary
    Given a conversation with a limit of 100 bytes
    And I add these conversation messages:
      | Role      | Content      |
      | user      | old question |
      | assistant | old answer   |
    And the AI model will answer "Summary of the conversation."
    When I compact the conversation
    Then the conversation holds 1 message
    And the conversation contains the message "Summary of the conversation."
