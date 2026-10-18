Feature: Recipe tools

  The tools are the public API the AI model calls. These tests drive the tool
  dispatcher directly, with no language model involved.

  Background:
    Given the recipe database contains a "Cable Coil" recipe at "Electronics Printer" with ingredients:
      | Ingredient | Amount |
      | Copper     | 5      |
      | Plastic    | 0.5    |
    And the recipe database contains "Steel Sheets"

  Scenario: Matching recipe names ignores case and excludes non-matches
    When I call the "search_recipes" tool with query "COIL"
    Then the tool succeeds
    And the tool result contains a recipe "Cable Coil"
    And the tool result does not contain a recipe "Steel Sheets"

  Scenario: The tool returns typed recipe data
    When I call the "search_recipes" tool with query "coil"
    Then the tool succeeds
    And the tool result contains a recipe "Cable Coil" with 2 ingredients
    And the recipe "Cable Coil" is made at "Electronics Printer"

  Scenario: The tool returns ingredients as CSV
    When I call the "search_recipes" tool with query "coil"
    Then the tool succeeds
    And the tool result is:
      """
      Name,Station,Ingredients
      Cable Coil,Electronics Printer,Copper x5; Plastic x0.5
      """

  Scenario: The tool rejects an unknown tool
    When I call the "no_such_tool" tool with query "coil"
    Then the tool fails with "Unknown tool 'no_such_tool'."

  Scenario: The tool requires its query argument
    When I call the "search_recipes" tool without arguments
    Then the tool fails with "Missing required argument 'query'."

  Scenario: No match reports the query
    When I call the "search_recipes" tool with query "glass"
    Then the tool succeeds
    And the tool result is:
      """
      No recipes found for 'glass'.
      """
