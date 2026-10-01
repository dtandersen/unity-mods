Feature: Status tool

  The get_status tool reports the player's current position and world.

  Background:
    Given my position is "1.5", "2.25", "-3" on "Europa"

  Scenario: Report the player's status
    When I call the "get_status" tool
    Then the tool succeeds
    And the status result is:
      | X   | Y    | Z  | World  |
      | 1.5 | 2.25 | -3 | Europa |

  Scenario: The status is returned as text
    When I call the "get_status" tool
    Then the tool succeeds
    And the tool result is:
      """
      Position: (1.5, 2.3, -3.0)
      World: Europa
      """
