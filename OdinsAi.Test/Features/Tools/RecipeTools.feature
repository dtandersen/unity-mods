Feature: Recipe tools

  The tools are the public API the AI model calls. These tests drive the tool
  dispatcher directly, with no language model involved.

  Background:
    Given the recipe database contains a "Turnip stew" recipe with ingredients:
      | Ingredient | Amount |
      | Turnip     | 3      |
      | Boar meat  | 1      |
    And the recipe database contains "Carrot soup"

  Scenario: Matching recipe names ignores case and excludes non-matches
    When I call the "search_recipes" tool with query "STEW"
    Then the tool succeeds
    And the tool result contains a recipe "Turnip stew"
    And the tool result does not contain a recipe "Carrot soup"

  Scenario: The tool returns typed recipe data
    When I call the "search_recipes" tool with query "stew"
    Then the tool succeeds
    And the tool result contains a recipe "Turnip stew" with 2 ingredients

  Scenario: The tool rejects an unknown tool
    When I call the "no_such_tool" tool with query "stew"
    Then the tool fails with "Unknown tool 'no_such_tool'."

  Scenario: The tool requires its query argument
    When I call the "search_recipes" tool without arguments
    Then the tool fails with "Missing required argument 'query'."

  Scenario: Locked recipes are flagged
    Given the recipe database contains a locked recipe "Secret stew"
    When I call the "search_recipes" tool with query "secret"
    Then the tool succeeds
    And the recipe "Secret stew" is locked