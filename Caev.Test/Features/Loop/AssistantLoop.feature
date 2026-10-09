Feature: Assistant loop

  The engine asks the model, runs requested tools, and feeds results back
  until the model answers. A stub tool stands in for real tools.

  Scenario: The answer comes back
    Given the AI model will answer "42"
    When the assistant is asked "what is the answer"
    Then the assistant replies "42"

  Scenario: A tool round trip feeds back to the model
    Given the AI model will request the tool "lookup" with query "cable"
    And the AI model will answer "Cable Coil needs 5 copper."
    When the assistant is asked "how do I make cable?"
    Then the assistant replies "Cable Coil needs 5 copper."
    And the tool "lookup" ran with query "cable"

  Scenario: The turn's instructions lead the messages
    Given the turn instructs "Look it up before answering"
    And the AI model will answer "42"
    When the assistant is asked "what is the answer"
    Then the instructions sent "Look it up before answering"

  Scenario: The turn's limit stops the tool rounds
    Given the turn allows 2 tool rounds
    And the AI model will request the tool "lookup" with query "cable"
    And the AI model will request the tool "lookup" with query "cable"
    When the assistant is asked "cable?"
    Then the assistant reports it could not complete

  Scenario: The turn reports the tool traffic
    Given the tool "lookup" will answer "Cable coil, 5 copper"
    And the AI model will request the tool "lookup" with query "cable"
    And the AI model will answer "Cable needs 5 copper."
    When the assistant is asked "how do I make cable?"
    Then the tool call reported "lookup(query=cable)"
    And the tool output reported "Cable coil, 5 copper"

  Scenario: The turn reports the model's reasoning
    Given the AI model reasons "weigh the options" and answers "Go north"
    When the assistant is asked "which way?"
    Then the reasoning reported "weigh the options"

  Scenario: The turn reports the answer
    Given the AI model will answer "Cable needs 5 copper."
    When the assistant is asked "how do I make cable?"
    Then the answer reported "Cable needs 5 copper."

  Scenario: The conversation compacts to a summary
    Given we discussed "cable" and heard "5 copper"
    And the AI model will answer "Cable needs copper."
    When the conversation is compacted
    Then the conversation holds the summary "Cable needs copper."

  Scenario: An unknown model gives a plain answer
    Given the model "invalid" is not a valid model
    When we ask the question "What time is it?"
    Then the agent answers with:
      """
      The model "invalid" is not available
      """

  Scenario: A failing model gives a plain answer
    Given the model "broken" returns a 500
    When we ask the question "What time is it?"
    Then the agent answers with:
      """
      The model "broken" returned an error
      """
