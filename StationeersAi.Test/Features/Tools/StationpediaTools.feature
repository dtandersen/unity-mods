Feature: Stationpedia tools

  The search_stationpedia and read_stationpedia tools expose the in-game
  encyclopedia so the assistant can answer questions about items, gases, and
  mechanics.

  Background:
    Given the stationpedia contains:
      | Key        | Title   | Description      | Text                                  |
      | ItemSteel  | Steel   | A sturdy alloy.  | Smelt iron and coal in a furnace.     |
      | GasOxygen  | Oxygen  | Breathe it.      | Oxygen supports combustion.           |

  Scenario: Search the stationpedia by title
    When I call the "search_stationpedia" tool with query "steel"
    Then the tool succeeds
    And the stationpedia result titles are "Steel"

  Scenario: Search the stationpedia by key
    When I call the "search_stationpedia" tool with query "GasOxygen"
    Then the tool succeeds
    And the stationpedia result titles are "Oxygen"

  Scenario: The stationpedia search requires its query argument
    When I call the "search_stationpedia" tool without arguments
    Then the tool fails with "Missing required argument 'query'."

  Scenario: Read a stationpedia page
    When I call the "read_stationpedia" tool with key "ItemSteel"
    Then the tool succeeds
    And the stationpedia page "ItemSteel" reads:
      """
      Steel
      Smelt iron and coal in a furnace.
      """

  Scenario: Reading an unknown key fails
    When I call the "read_stationpedia" tool with key "Nope"
    Then the tool fails with "No Stationpedia page found for key 'Nope'."

  Scenario: Read requires its key argument
    When I call the "read_stationpedia" tool without arguments
    Then the tool fails with "Missing required argument 'key'."

  Scenario: Read lists what uses the subject
    Given the stationpedia contains:
      | Key           | Title | Description   | Text              |
      | ThingItemCoal | Coal  | Burns hot.    | Dug from asteroids. |
    And the stationpedia page "ThingItemCoal" is used in:
      | Title                | Detail        | Key                          |
      | Furnace                |               | ThingStructureFurnace        |
      | Generator (Solid Fuel) | 100 kW Energy | ThingStructureSolidGenerator |
    When I call the "read_stationpedia" tool with key "ThingItemCoal"
    Then the tool succeeds
    And the tool result is:
      """
      Coal
      Dug from asteroids.
      Used in:
      Furnace
      Generator (Solid Fuel) - 100 kW Energy
      """

  Scenario: Read lists how to manufacture the subject
    Given the stationpedia contains:
      | Key        | Title | Description | Text            |
      | ItemSteel2 | Steel | Strong alloy. | Smelt to shape. |
    And the stationpedia page "ItemSteel2" is made in:
      | Station          | Tier     | Detail                                       | Key                       |
      | Advanced Furnace | Tier One | Temperature: 900 K to 100 kK; Iron x0.75    | ThingAdvancedFurnace      |
      | Furnace          |          | Iron x0.5                                  | ThingStructureFurnace     |
    When I call the "read_stationpedia" tool with key "ItemSteel2"
    Then the tool succeeds
    And the tool result is:
      """
      Steel
      Smelt to shape.
      Made in:
      Advanced Furnace (Tier One) - Temperature: 900 K to 100 kK; Iron x0.75
      Furnace - Iron x0.5
      """
