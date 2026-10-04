Feature: Ask question

  Scenario: User asks a question with the console
    Given the AI model will answer "it is noon"
    When the user types the command "ai what time is it?"
    Then the question "what time is it?" is queued
    And the console displays:
      """
      Question queued
      """
