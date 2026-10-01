Feature: World tool

  The get_world tool reports the world, orbit, sunlight, and outside air.

  Scenario: Report the world
    Given a world
    When I call the "get_world" tool
    Then the tool succeeds
    And the tool result contains "World: Europa"
    And the tool result contains "Day length: 20m"
    And the tool result contains "500 W/m"
    And the tool result contains "Nitrogen 90%"

  Scenario: No world is loaded
    Given no world is loaded
    When I call the "get_world" tool
    Then the tool fails with "World info is unavailable."
