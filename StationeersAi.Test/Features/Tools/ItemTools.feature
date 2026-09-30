Feature: Item tools

  The search_items tool looks up items in the game's item list and returns their
  type, so the assistant can recognise materials it doesn't otherwise know.

  Background:
    Given the item database contains:
      | Name        | Type  |
      | Steel       | Ingot |
      | Steel Sheets| Ingot |
      | Copper Ore  | Ore   |

  Scenario: Look up an item by name
    When I call the "search_items" tool with query "steel sheets"
    Then the tool succeeds
    And the item result is:
      | Name         | Type  |
      | Steel Sheets | Ingot |

  Scenario: Matching item names ignores case and excludes non-matches
    When I call the "search_items" tool with query "STEEL"
    Then the tool succeeds
    And the tool result contains:
      """
      Name,Type,Id
      Steel,Ingot,Steel
      Steel Sheets,Ingot,Steel Sheets
      """

  Scenario: The item tool requires its query argument
    When I call the "search_items" tool without arguments
    Then the tool fails with "Missing required argument 'query'."

  Scenario: The item tool reports an unknown tool
    When I call the "no_such_tool" tool with query "steel"
    Then the tool fails with "Unknown tool 'no_such_tool'."
