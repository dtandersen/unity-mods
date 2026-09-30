Feature: Inventory tools

  The search_inventory tool searches the player's carried items and reports
  each matching stack with its quantity and the slot it is worn in.

  Background:
    Given my inventory contains:
      | Item   | Quantity | Slot        |
      | Welder | 85%      | Right hand  |
      | Steel  | x10      | Backpack    |
      | Steel  | x4       | Toolbelt    |

  Scenario: Search carried items by name, ignoring case
    When I call the "search_inventory" tool with query "STEEL"
    Then the tool succeeds
    And the inventory result is:
      | Name  | Quantity | Slot     |
      | Steel | x10      | Backpack |
      | Steel | x4       | Toolbelt |

  Scenario: The inventory tool requires its query argument
    When I call the "search_inventory" tool without arguments
    Then the tool fails with "Missing required argument 'query'."

  Scenario: No match reports the query
    When I call the "search_inventory" tool with query "helmet"
    Then the tool succeeds
    And the tool result contains "No carried items found for 'helmet'."
