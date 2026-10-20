Feature: Conversation context

  The assistant keeps a conversation history between questions so the user can
  refer back to earlier turns. The history has a size limit and can be compacted
  into a summary.

  Scenario: History is sent with the next question
    Given the AI model will answer "First answer."
    And the AI model will answer "Second answer."
    When I ask the assistant "first question" with context
    And I ask the assistant "second question" with context
    Then the AI model's last request contains a "user" message "first question"
    And the AI model's last request contains a "assistant" message "First answer."
    And the AI model's last request contains a "user" message "second question"

  Scenario: The conversation keeps only the newest messages
    Given a conversation with a limit of 4 messages
    And I add these conversation messages:
      | Role      | Content |
      | user      | m1      |
      | assistant | m2      |
      | user      | m3      |
      | assistant | m4      |
      | user      | m5      |
      | assistant | m6      |
    Then the conversation size is 4
    And the conversation contains the message "m3"
    And the conversation does not contain the message "m2"

  Scenario: Clearing empties the conversation
    Given a conversation with a limit of 8 messages
    And I add these conversation messages:
      | Role | Content |
      | user | hello   |
    When I clear the conversation
    Then the conversation size is 0

  Scenario: Compacting replaces the context with a summary
    Given a conversation with a limit of 8 messages
    And I add these conversation messages:
      | Role      | Content      |
      | user      | old question |
      | assistant | old answer   |
    And the AI model will answer "Summary of the conversation."
    When I compact the conversation
    Then the conversation size is 1
    And the conversation contains the message "Summary of the conversation."