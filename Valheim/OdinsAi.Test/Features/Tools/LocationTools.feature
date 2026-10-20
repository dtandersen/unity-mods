Feature: Location tool

  The get_location tool reports the player's current position and biome.

  Background:
    Given my position is "123", "45.5", "-67" in the "Black Forest" biome

  Scenario: Report the player's location
    When I call the "get_location" tool
    Then the tool succeeds
    And the location result is:
      | X   | Y    | Z   | Biome        |
      | 123 | 45.5 | -67 | Black Forest |

  Scenario: The location is returned as CSV
    When I call the "get_location" tool
    Then the tool succeeds
    And the tool result contains:
      """
      X,Y,Z,Biome
      123,45.5,-67,Black Forest
      """