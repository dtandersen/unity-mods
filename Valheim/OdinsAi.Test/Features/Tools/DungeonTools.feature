Feature: Dungeon tools

  The search_dungeons tool lists dungeon entrances near the player, nearest first,
  with distance and compass direction.

  Background:
    Given the nearby dungeons are:
      | Name        | Distance | Direction |
      | SunkenCrypt | 500      | N         |
      | Crypt       | 42       | NE        |
      | TrollCave   | 120      | SW        |

  Scenario: List nearby dungeons nearest first
    When I call the "search_dungeons" tool
    Then the tool succeeds
    And the dungeon result is:
      | Name        | Distance | Direction |
      | Crypt       | 42       | NE        |
      | TrollCave   | 120      | SW        |
      | SunkenCrypt | 500      | N         |

  Scenario: Filter dungeons by name
    When I call the "search_dungeons" tool with query "crypt"
    Then the tool succeeds
    And the dungeon result is:
      | Name        | Distance | Direction |
      | Crypt       | 42       | NE        |
      | SunkenCrypt | 500      | N         |