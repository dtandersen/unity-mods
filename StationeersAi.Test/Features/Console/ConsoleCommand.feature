Feature: Console command

  Scenario: User asks a question with the console
    Given the AI model will answer "it is noon"
    When the user types the command "ai what time is it?"
    Then the question "what time is it?" is queued
    And the console displays:
      """
      Question queued
      """

  Scenario: Show the SAGE window
    Given the SAGE window isn't visible
    When the user types the command "ai window"
    Then the SAGE window is visible

  Scenario: Clear the context
    Given the AI agent has context
    When the user types the command "ai clear"
    Then the AI agent has an empty context

  Scenario: Compact the context
    Given the AI agent has context
    And the AI model will answer "it is noon"
    When the user types the command "ai compact"
    Then the AI agent compacts its context
    Then the console displays:
      """
      Compacting context...
      """
