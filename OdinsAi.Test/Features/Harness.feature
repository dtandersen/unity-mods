Feature: Test harness

  As a developer
  I want the BDD harness to run
  So that Odin's Eye requirements can be expressed as feature files

  Scenario: The BDD harness executes
    Given the Odin's Eye core is loaded
    Then the core assembly name should be "OdinsAi.Core"