Feature: Upgrade tool

  The search_upgrades tool reports what it costs to upgrade an item at each
  quality. search_recipes only covers the base crafting cost. An upgrade is
  unlocked when the recipe is known and a station of the required level is
  nearby.

  Background:
    Given the recipe database contains a "Bronze Axe" recipe with upgrades:
      | Quality | Station | StationLevel | Ingredients |
      | 2       | Forge   | 1            | Wood x4; Bronze x2 |
      | 3       | Forge   | 2            | Wood x8; Bronze x4; Leather Scraps x2 |
    And a "Forge" station of level 2 is nearby

  Scenario: Show the upgrade costs by quality
    When I call the "search_upgrades" tool with query "bronze axe"
    Then the tool succeeds
    And the upgrade result is:
      | Item       | Quality | Station | StationLevel | Ingredients | Unlocked |
      | Bronze Axe | 2       | Forge   | 1            | Wood x4; Bronze x2 | Yes |
      | Bronze Axe | 3       | Forge   | 2            | Wood x8; Bronze x4; Leather Scraps x2 | Yes |

  Scenario: An upgrade without a nearby station of the required level is locked
    Given the recipe database contains a "Black Metal Axe" recipe with upgrades:
      | Quality | Station     | StationLevel | Ingredients |
      | 2       | Black Forge | 3            | Black Metal x20 |
    When I call the "search_upgrades" tool with query "black metal axe"
    Then the tool succeeds
    And the upgrade result is:
      | Item            | Quality | Station     | StationLevel | Ingredients | Unlocked |
      | Black Metal Axe | 2       | Black Forge | 3            | Black Metal x20 | No |

  Scenario: Items with no upgrades are not listed
    Given the recipe database contains "Mead"
    When I call the "search_upgrades" tool with query "mead"
    Then the tool succeeds
    And the tool result contains:
      """
      No upgrade costs found for 'mead'.
      """

  Scenario: The upgrade tool requires its query argument
    When I call the "search_upgrades" tool without arguments
    Then the tool fails with "Missing required argument 'query'."