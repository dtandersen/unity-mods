Feature: Structure tools

  The search_structures tool looks up buildable structures so the assistant can
  answer "what do I build for X" questions.

  Background:
    Given the structure database contains:
      | Name               | Description       |
      | Solar Panel        | Generates power.  |
      | Solid Fuel Generator | Burns coal.     |

  Scenario: Look up a structure by name
    When I call the "search_structures" tool with query "solar"
    Then the tool succeeds
    And the tool result is:
      """
      Name: Solar Panel
      Description: Generates power.
      Steps:
      """

  Scenario: Look up what construction needs
    Given the structure database contains:
      | Name    | Description |
      | Furnace | Smelts ore. |
    And the structure "Furnace" is built in steps:
      | State | Tool          | Materials      |
      | 1     |               | Kit (Furnace) x1 |
      | 2     | Wrench        | Iron Sheets x2 |
      | 3     | Welding Torch | Iron Sheets x2 |
    When I call the "search_structures" tool with query "furnace"
    Then the tool succeeds
    And the tool result is:
      """
      Name: Furnace
      Description: Smelts ore.
      Steps:
      1. Kit (Furnace) x1
      2. Wrench - Iron Sheets x2
      3. Welding Torch - Iron Sheets x2
      """

  Scenario: A structure with several materials lists each one
    Given the structure database contains:
      | Name      | Description   |
      | Autolathe | Prints parts. |
    And the structure "Autolathe" is built in steps:
      | State | Tool | Materials                                        |
      | 1     |      | Iron Sheets x4; Copper Wire x2; Glass Sheets x1 |
    When I call the "search_structures" tool with query "autolathe"
    Then the tool succeeds
    And the tool result is:
      """
      Name: Autolathe
      Description: Prints parts.
      Steps:
      1. Iron Sheets x4; Copper Wire x2; Glass Sheets x1
      """

  Scenario: The structure tool requires its query argument
    When I call the "search_structures" tool without arguments
    Then the tool fails with "Missing required argument 'query'."

  Scenario: No match reports the query
    When I call the "search_structures" tool with query "airlock"
    Then the tool succeeds
    And the tool result is:
      """
      No structures found for 'airlock'.
      """
