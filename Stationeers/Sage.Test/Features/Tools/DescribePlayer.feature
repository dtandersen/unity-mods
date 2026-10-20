Feature: Describe player tool

  The describe_player tool reports the player's current position, world, and room.

  Background:
    Given my position is "1.5", "2.25", "-3" on "Europa"

  Scenario: Report the player's status
    When I call the "describe_player" tool
    Then the tool succeeds
    And the status result is:
      | X   | Y    | Z  | World  |
      | 1.5 | 2.25 | -3 | Europa |

  Scenario: The status is returned as text
    When I call the "describe_player" tool
    Then the tool succeeds
    And the tool result is:
      """
      Position: (1.5, 2.3, -3.0)
      World: Europa
      Room: outside
      """

  Scenario: No position is unavailable
    Given my position is unknown
    When I call the "describe_player" tool
    Then the tool fails with "Player status is unavailable."
