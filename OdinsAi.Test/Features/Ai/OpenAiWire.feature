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
      {"choices":[{"message":{"tool_calls":[{"id":"call_1","type":"function","function":{"name":"search_food","arguments":"{\"query\":\"cooked\"}"}}]}}]}
      """
    When I complete a chat with "What food is good?" with the "search_food" tool
    Then the completion requests the tool "search_food"
    And the tool arguments contain "query" = "cooked"
    And the request body contains "search_food"
    And the request body contains "query"

  Scenario: A server error is surfaced
    Given the server will reply 401 with:
      """
      {"error":"unauthorized"}
      """
    When I complete a chat with "Hi" using model "test-model"
    Then the completion fails with "401"