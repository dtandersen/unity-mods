Feature: Resource tools

  The search_resources tool lists resource nodes near the player (within 100
  metres), nearest first, with type, distance and compass direction.

  Background:
    Given the nearby resources are:
      | Name        | Type     | Distance | Direction |
      | Tin deposit | ore      | 25       | E         |
      | Mushroom    | pickable | 12       | N         |
      | Beech1      | tree     | 60       | SW        |
      | Copper vein | ore      | 300      | S         |

  Scenario: List nearby resources nearest first
    When I call the "search_resources" tool
    Then the tool succeeds
    And the resource result is:
      | Name        | Type     | Distance | Direction |
      | Mushroom    | pickable | 12       | N         |
      | Tin deposit | ore      | 25       | E         |
      | Beech1      | tree     | 60       | SW        |

  Scenario: Filter resources by name
    When I call the "search_resources" tool with query "tin"
    Then the tool succeeds
    And the resource result is:
      | Name        | Type | Distance | Direction |
      | Tin deposit | ore  | 25       | E         |