Feature: Device search tool

  The search_devices tool lists live logic devices: labels, prefab types,
  reference ids, and data networks. An optional computer limits the list to
  its data network.

  Scenario: Search devices by name
    Given the world has devices
    When I call the "search_devices" tool with query "vent"
    Then the tool succeeds
    And the tool result is:
      """
      | Name     | Type             | ReferenceId | Network |
      | -------- | ---------------- | ----------- | ------- |
      | Air Vent | StructureAirVent |         101 |       7 |
      """

  Scenario: No match
    Given the world has devices
    When I call the "search_devices" tool with query "nope"
    Then the tool succeeds
    And the tool result is:
      """
      No devices found for 'nope'.
      """
