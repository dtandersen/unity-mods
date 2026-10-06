Feature: Inventory tools

  The search_inventory tool searches the player's carried items and reports
  each matching stack with its quantity, the slot it is worn in, and which
  hand holds it when one does.

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
      | Name  | Quantity | Slot     | Hand |
      | Steel | x10      | Backpack |      |
      | Steel | x4       | Toolbelt |      |

  Scenario: Blank query lists all
    When I call the "search_inventory" tool
    Then the tool succeeds
    And the tool result is:
      """
      Name,Quantity,Slot,Hand
      Steel,x10,Backpack,
      Steel,x4,Toolbelt,
      Welder,85%,Right hand,Right
      """

  Scenario: Blank query with explicit empty string lists all
    When I call the "search_inventory" tool with query ""
    Then the tool succeeds
    And the tool result is:
      """
      Name,Quantity,Slot,Hand
      Steel,x10,Backpack,
      Steel,x4,Toolbelt,
      Welder,85%,Right hand,Right
      """

  Scenario: No match reports the query
    When I call the "search_inventory" tool with query "helmet"
    Then the tool succeeds
    And the tool result is:
      """
      No carried items found for 'helmet'.
      """

  Scenario: Held items report the hand
    When I call the "search_inventory" tool with query "welder"
    Then the tool succeeds
    And the inventory result is:
      | Name   | Quantity | Slot       | Hand  |
      | Welder | 85%      | Right hand | Right |
