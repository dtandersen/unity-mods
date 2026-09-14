Feature: Item tools

  The search_items tool looks up items in the game's item list and returns their
  type, so the assistant can recognise ingredients it doesn't otherwise know.

  Background:
    Given the item database contains:
      | Name               | Type            |
      | Bronze Battle Idol | material        |
      | Cooked meat        | food            |
      | Iron Atgeir        | twohandedweapon |

  Scenario: Look up an item by name
    When I call the "search_items" tool with query "battle idol"
    Then the tool succeeds
    And the item result is:
      | Name               | Type     |
      | Bronze Battle Idol | material |

  Scenario: The item tool requires its query argument
    When I call the "search_items" tool without arguments
    Then the tool fails with "Missing required argument 'query'."