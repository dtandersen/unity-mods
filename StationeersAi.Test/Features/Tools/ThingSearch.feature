Feature: Thing search tool

  The search_things tool lists loose world things the device search never
  sees: ores, ingots, plants, dropped items. A query filters by name, an
  origin ("x, y, z", defaulting to your position) and a distance in meters
  keep only what is near. Nearest first, with the bearing in degrees
  from the origin.

  Background:
    Given these world things exist:
      | Name       | Type | Thing | X   | Y  | Z | Room |
      | Iron Ore   | Ore  | 101   | 3   | 4  | 0 | 5    |
      | Copper Ore | Ore  | 102   | -30 | 40 | 0 | 5    |
    And my position is "0", "0", "0" on "world"
    And I face "0"

  Scenario: Search things by name near an origin
    When I call the "search_things" tool with query "ore" from "0, 0, 0" within "100"
    Then the tool succeeds
    And the tool result is:
      """
      | Name       | Type | Thing | Distance | Direction | Room |
      | ---------- | ---- | ----- | -------- | --------- | ---- |
      | Iron Ore   | Ore  |   101 | 5 m      |       +90 |    5 |
      | Copper Ore | Ore  |   102 | 50 m     |       -90 |    5 |
      Showing 2 of 2.
      """

  Scenario: Distance keeps only near things
    When I call the "search_things" tool with query "ore" from "0, 0, 0" within "10"
    Then the tool succeeds
    And the tool result contains "Iron Ore"
    And the tool result contains "Showing 1 of 1."

  Scenario: Things outside rooms show no room
    Given these world things exist:
      | Name        | Type  | Thing | X | Y  | Z | Room |
      | Steel Ingot | Ingot | 103   | 0 | 10 | 0 | 0    |
    When I call the "search_things" tool with query "ingot" from "0, 0, 0" within "100"
    Then the tool succeeds
    And the tool result is:
      """
      | Name        | Type  | Thing | Distance | Direction | Room |
      | ----------- | ----- | ----- | -------- | --------- | ---- |
      | Steel Ingot | Ingot |   103 | 10 m     | here      | none |
      Showing 1 of 1.
      """

  Scenario: An omitted origin uses the player's position
    When I call the "search_things" tool with query "ore"
    Then the tool succeeds
    And the tool result contains "5 m"

  Scenario: An origin must look like x, y, z
    When I call the "search_things" tool with query "ore" from "abc" within "10"
    Then the tool fails with "Origin 'abc' is not 'x, y, z'."

  Scenario: A distance must be a number
    When I call the "search_things" tool with query "ore" from "0, 0, 0" within "far"
    Then the tool fails with "Distance 'far' is not a number."

  Scenario: A distance cannot be negative
    When I call the "search_things" tool with query "ore" from "0, 0, 0" within "-5"
    Then the tool fails with "Distance '-5' must be zero or more."
