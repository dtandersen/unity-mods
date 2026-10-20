Feature: World tool

  The get_world tool reports the world, orbit, sunlight, and outside air.

  Scenario: Report the world
    Given a world
    When I call the "get_world" tool
    Then the tool succeeds
    And the tool result is:
      """
      World: Europa
      Gravity: 9.8 m/s², Day 12, Lava: no
      Day length: 20m, Year: 60d (72.0 days per year)
      Orbit: e=0.01, a=5.20 AU, inclination 3.1°
      Sun: 500 W/m² (0.37x Earth), time 35%
      Outside air: -20°C, 0 kPa
      Air mix: Nitrogen 90%, Oxygen 10%
      """

  Scenario: No world is loaded
    Given no world is loaded
    When I call the "get_world" tool
    Then the tool fails with "World info is unavailable."
