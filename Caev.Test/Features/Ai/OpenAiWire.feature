Feature: OpenAI chat client over HTTP

  The OpenAI client talks to an OpenAI-compatible endpoint over a real socket. A
  local HTTP server stands in for the model provider, so we exercise request
  building, the wire format, and response parsing without a live model.

  Background:
    Given a local AI server

  Scenario: A content reply is parsed
    Given the server will reply 200 with:
      """
      {"choices":[{"message":{"role":"assistant","content":"Hello"}}]}
      """
    When I complete a chat with "Hi" using model "test-model"
    Then the completion content is "Hello"
    And the server received a request to "/v1/chat/completions"
    And the request body contains "test-model"

  Scenario: A tool-call reply is parsed and the schema is sent
    Given the server will reply 200 with:
      """
      {"choices":[{"message":{"tool_calls":[{"id":"call_1","type":"function","function":{"name":"lookup","arguments":"{\"query\":\"coil\"}"}}]}}]}
      """
    When I complete a chat with "How do I make cable?" with the "lookup" tool
    Then the completion requests the tool "lookup"
    And the tool arguments contain "query" = "coil"
    And the request body contains "lookup"
    And the request body contains "query"

  Scenario: A server error is surfaced
    Given the server will reply 401 with:
      """
      {"error":"unauthorized"}
      """
    When I complete a chat with "Hi" using model "test-model"
    Then the completion fails with "401"

  Scenario: A 503 is retried and then succeeds
    Given the server will reply 503 with:
      """
      {"error":"busy"}
      """
    And the server will reply 503 with:
      """
      {"error":"busy"}
      """
    And the server will reply 200 with:
      """
      {"choices":[{"message":{"role":"assistant","content":"Hello"}}]}
      """
    When I complete a chat with "Hi" using model "test-model"
    Then the completion content is "Hello"

  Scenario: A reasoning reply is parsed
    Given the server will reply 200 with:
      """
      {"choices":[{"message":{"role":"assistant","content":"42","reasoning_content":"I checked the manifest."}}]}
      """
    When I complete a chat with "Hi" using model "test-model"
    Then the completion content is "42"
    And the completion reasoning is "I checked the manifest."

  Scenario: The reasoning effort is sent when configured
    Given the server will reply 200 with:
      """
      {"choices":[{"message":{"role":"assistant","content":"ok"}}]}
      """
    When I complete a chat with "Hi" using reasoning effort "high"
    Then the request body contains "reasoning_effort"
