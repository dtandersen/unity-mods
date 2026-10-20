Feature: Creature tools

  The search_creatures tool lists creatures near the player (within 100 metres),
  nearest first, with distance and compass direction.

  Background:
    Given the nearby creatures are:
      | Name      | Distance | Direction |
      | Greydwarf | 18       | NE        |
      | Deer      | 40       | S         |
      | Troll     | 80       | W         |
      | Skeleton  | 220      | N         |

  Scenario: List nearby creatures nearest first
    When I call the "search_creatures" tool
    Then the tool succeeds
    And the creature result is:
      | Name      | Distance | Direction |
      | Greydwarf | 18       | NE        |
      | Deer      | 40       | S         |
      | Troll     | 80       | W         |

  Scenario: Filter creatures by name
    When I call the "search_creatures" tool with query "troll"
    Then the tool succeeds
    And the creature result is:
      | Name  | Distance | Direction |
      | Troll | 80       | W         |