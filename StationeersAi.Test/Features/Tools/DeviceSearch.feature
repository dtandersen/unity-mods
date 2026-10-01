Feature: Device search tool

  The search_devices tool lists built things: devices, pipes, walls. A query
  filters by name, a network id limits the list to one data network, and the
  logic flag keeps only logic speakers or only dumb objects.

  Scenario: List all devices
    Given the world has devices
    When I call the "search_devices" tool
    Then the tool succeeds
    And the tool result is:
      """
      | Name       | Type               | Thing | Network | Logic |
      | ---------- | ------------------ | ----- | ------- | ----- |
      | Air Vent   | StructureAirVent   |   101 |       7 | yes   |
      | Gas Sensor | StructureGasSensor |   102 | none    | yes   |
      | Iron Pipe  | StructurePipe      |   103 | none    | no    |
      """

  Scenario: Search devices by name
    Given the world has devices
    When I call the "search_devices" tool with query "vent"
    Then the tool succeeds
    And the tool result is:
      """
      | Name     | Type             | Thing | Network | Logic |
      | -------- | ---------------- | ----- | ------- | ----- |
      | Air Vent | StructureAirVent |   101 |       7 | yes   |
      """

  Scenario: Search one network
    Given the world has devices
    When I search devices on network "7"
    Then the tool succeeds
    And the search used network "7"

  Scenario: Search by name on one network
    Given the world has devices
    When I search devices for "vent" on network "7"
    Then the tool succeeds
    And the search used network "7"
    And the tool result is:
      """
      | Name     | Type             | Thing | Network | Logic |
      | -------- | ---------------- | ----- | ------- | ----- |
      | Air Vent | StructureAirVent |   101 |       7 | yes   |
      """

  Scenario: Only logic devices
    Given the world has devices
    When I search devices with logic "true"
    Then the tool succeeds
    And the tool result is:
      """
      | Name       | Type               | Thing | Network | Logic |
      | ---------- | ------------------ | ----- | ------- | ----- |
      | Air Vent   | StructureAirVent   |   101 |       7 | yes   |
      | Gas Sensor | StructureGasSensor |   102 | none    | yes   |
      """

  Scenario: Only dumb objects
    Given the world has devices
    When I search devices with logic "false"
    Then the tool succeeds
    And the tool result is:
      """
      | Name      | Type          | Thing | Network | Logic |
      | --------- | ------------- | ----- | ------- | ----- |
      | Iron Pipe | StructurePipe |   103 | none    | no    |
      """

  Scenario: No match
    Given the world has devices
    When I call the "search_devices" tool with query "nope"
    Then the tool succeeds
    And the tool result is:
      """
      No devices found for 'nope'.
      """
