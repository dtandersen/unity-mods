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
      | Name       | Type               | Thing | Network | Logic | Room |
      | ---------- | ------------------ | ----- | ------- | ----- | ---- |
      | Air Vent   | StructureAirVent   |   101 |       7 | yes   |    5 |
      | Gas Sensor | StructureGasSensor |   102 | none    | yes   |    5 |
      | Iron Pipe  | StructurePipe      |   103 | none    | no    | none |
      Showing 3 of 3.
      """

  Scenario: Search devices by name
    Given the world has devices
    When I call the "search_devices" tool with query "vent"
    Then the tool succeeds
    And the tool result is:
      """
      | Name     | Type             | Thing | Network | Logic | Room |
      | -------- | ---------------- | ----- | ------- | ----- | ---- |
      | Air Vent | StructureAirVent |   101 |       7 | yes   |    5 |
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
      | Name     | Type             | Thing | Network | Logic | Room |
      | -------- | ---------------- | ----- | ------- | ----- | ---- |
      | Air Vent | StructureAirVent |   101 |       7 | yes   |    5 |
      Showing 1 of 1.
      """

  Scenario: Only logic devices
    Given the world has devices
    When I search devices with logic "true"
    Then the tool succeeds
    And the tool result is:
      """
      | Name       | Type               | Thing | Network | Logic | Room |
      | ---------- | ------------------ | ----- | ------- | ----- | ---- |
      | Air Vent   | StructureAirVent   |   101 |       7 | yes   |    5 |
      | Gas Sensor | StructureGasSensor |   102 | none    | yes   |    5 |
      Showing 2 of 2.
      """

  Scenario: Only dumb objects
    Given the world has devices
    When I search devices with logic "false"
    Then the tool succeeds
    And the tool result is:
      """
      | Name      | Type          | Thing | Network | Logic | Room |
      | --------- | ------------- | ----- | ------- | ----- | ---- |
      | Iron Pipe | StructurePipe |   103 | none    | no    | none |
      Showing 1 of 1.
      """

  Scenario: Search one room
    Given the world has devices
    When I search devices in room "5"
    Then the tool succeeds
    And the search used room "5"
    And the tool result is:
      """
      | Name       | Type               | Thing | Network | Logic | Room |
      | ---------- | ------------------ | ----- | ------- | ----- | ---- |
      | Air Vent   | StructureAirVent   |   101 |       7 | yes   |    5 |
      | Gas Sensor | StructureGasSensor |   102 | none    | yes   |    5 |
      Showing 2 of 2.
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
      | Name      | Type          | Thing | Network | Logic | Room |
      | --------- | ------------- | ----- | ------- | ----- | ---- |
      | Device 01 | StructurePipe |   101 | none    | no    | none |
      | Device 02 | StructurePipe |   102 | none    | no    | none |
      | Device 03 | StructurePipe |   103 | none    | no    | none |
      | Device 04 | StructurePipe |   104 | none    | no    | none |
      | Device 05 | StructurePipe |   105 | none    | no    | none |
      | Device 06 | StructurePipe |   106 | none    | no    | none |
      | Device 07 | StructurePipe |   107 | none    | no    | none |
      | Device 08 | StructurePipe |   108 | none    | no    | none |
      | Device 09 | StructurePipe |   109 | none    | no    | none |
      | Device 10 | StructurePipe |   110 | none    | no    | none |
      Showing 10 of 12.
      """

  Scenario: Logic devices sort first
    Given the world has cables plus a heater
    When I call the "search_devices" tool
    Then the tool succeeds
    And the tool result is:
      """
      | Name        | Type                | Thing | Network | Logic | Room |
      | ----------- | ------------------- | ----- | ------- | ----- | ---- |
      | Wall Heater | StructureWallHeater |  1208 |    1194 | yes   |    5 |
      | Cable 01    | StructureCable      |   201 | none    | no    | none |
      | Cable 02    | StructureCable      |   202 | none    | no    | none |
      | Cable 03    | StructureCable      |   203 | none    | no    | none |
      | Cable 04    | StructureCable      |   204 | none    | no    | none |
      | Cable 05    | StructureCable      |   205 | none    | no    | none |
      | Cable 06    | StructureCable      |   206 | none    | no    | none |
      | Cable 07    | StructureCable      |   207 | none    | no    | none |
      | Cable 08    | StructureCable      |   208 | none    | no    | none |
      | Cable 09    | StructureCable      |   209 | none    | no    | none |
      Showing 10 of 12.
      """

  Scenario: Offset pages past the cap
    Given the world has 12 devices
    When I search devices with offset "10"
    Then the tool succeeds
    And the tool result is:
      """
      | Name      | Type          | Thing | Network | Logic | Room |
      | --------- | ------------- | ----- | ------- | ----- | ---- |
      | Device 11 | StructurePipe |   111 | none    | no    | none |
      | Device 12 | StructurePipe |   112 | none    | no    | none |
      Showing 2 of 12.
      """

  Scenario: Logic filter raises the cap
    Given the world has 12 logic devices
    When I search devices with logic "true"
    Then the tool succeeds
    And the tool found 12 devices
