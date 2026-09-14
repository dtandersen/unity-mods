Feature: Inventory tools

  The search_inventory tool searches the player's carried items and returns the
  total amount of each matching item, not each individual stack. It can match on
  name and/or on a coarse type such as "food".

  Background:
    Given my inventory contains:
      | Item         | Amount | Type     |
      | Turnip       | 5      | material |
      | Turnip       | 3      | material |
      | Turnip seeds | 2      | material |
      | Cooked meat  | 4      | food     |
      | Wood         | 50     | material |

  Scenario: Search totals matching items by name, ignoring case
    When I call the "search_inventory" tool with query "TURNIP"
    Then the tool succeeds
    And the inventory result contains "Turnip" x8
    And the inventory result contains "Turnip seeds" x2
    And the inventory result does not contain "Wood"

  Scenario: Filter carried items by type
    When I call the "search_inventory" tool with type "food"
    Then the tool succeeds
    And the inventory result contains "Cooked meat" x4
    And the inventory result does not contain "Turnip"

  Scenario: The inventory tool needs a query or a type
    When I call the "search_inventory" tool without arguments
    Then the tool fails with "Provide a 'query' and/or a 'type'."