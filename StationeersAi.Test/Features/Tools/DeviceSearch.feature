Feature: Device search tool

  The search_devices tool lists built things: devices, pipes, walls. A query
  filters by name, a network id limits the list to one data network, and the
  logic flag keeps only logic speakers or only dumb objects. Logic devices
  list first, 10 at a time (30 for logic-only); offset pages past the cap.

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
      Showing 3 of 3.
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
      Showing 1 of 1.
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
      Showing 1 of 1.
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
      Showing 2 of 2.
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
      Showing 1 of 1.
      """

  Scenario: No match
    Given the world has devices
    When I call the "search_devices" tool with query "nope"
    Then the tool succeeds
    And the tool result is:
      """
      No devices found for 'nope'.
      """

  Scenario: Blank search states cap and total
    Given the world has 12 devices
    When I call the "search_devices" tool
    Then the tool succeeds
    And the tool result is:
      """
      | Name      | Type          | Thing | Network | Logic |
      | --------- | ------------- | ----- | ------- | ----- |
      | Device 01 | StructurePipe |   101 | none    | no    |
      | Device 02 | StructurePipe |   102 | none    | no    |
      | Device 03 | StructurePipe |   103 | none    | no    |
      | Device 04 | StructurePipe |   104 | none    | no    |
      | Device 05 | StructurePipe |   105 | none    | no    |
      | Device 06 | StructurePipe |   106 | none    | no    |
      | Device 07 | StructurePipe |   107 | none    | no    |
      | Device 08 | StructurePipe |   108 | none    | no    |
      | Device 09 | StructurePipe |   109 | none    | no    |
      | Device 10 | StructurePipe |   110 | none    | no    |
      Showing 10 of 12.
      """

  Scenario: Logic devices sort first
    Given the world has cables plus a heater
    When I call the "search_devices" tool
    Then the tool succeeds
    And the tool result is:
      """
      | Name        | Type                | Thing | Network | Logic |
      | ----------- | ------------------- | ----- | ------- | ----- |
      | Wall Heater | StructureWallHeater |  1208 |    1194 | yes   |
      | Cable 01    | StructureCable      |   201 | none    | no    |
      | Cable 02    | StructureCable      |   202 | none    | no    |
      | Cable 03    | StructureCable      |   203 | none    | no    |
      | Cable 04    | StructureCable      |   204 | none    | no    |
      | Cable 05    | StructureCable      |   205 | none    | no    |
      | Cable 06    | StructureCable      |   206 | none    | no    |
      | Cable 07    | StructureCable      |   207 | none    | no    |
      | Cable 08    | StructureCable      |   208 | none    | no    |
      | Cable 09    | StructureCable      |   209 | none    | no    |
      Showing 10 of 12.
      """

  Scenario: Offset pages past the cap
    Given the world has 12 devices
    When I search devices with offset "10"
    Then the tool succeeds
    And the tool result is:
      """
      | Name      | Type          | Thing | Network | Logic |
      | --------- | ------------- | ----- | ------- | ----- |
      | Device 11 | StructurePipe |   111 | none    | no    |
      | Device 12 | StructurePipe |   112 | none    | no    |
      Showing 2 of 12.
      """

  Scenario: Logic filter raises the cap
    Given the world has 12 logic devices
    When I search devices with logic "true"
    Then the tool succeeds
    And the tool found 12 devices
