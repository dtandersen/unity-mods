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
