Feature: Structure search tool

  The search_structures tool lists built things: structures, pipes, walls. A query
  filters by name, a cable network id limits the list to one cable network, and the
  logic flag keeps only logic speakers or only dumb objects. Logic structures
  list first, 10 at a time (30 for logic-only); offset pages past the cap.

  Scenario: List all structures
    Given the world has structures
    When I call the "search_structures" tool
    Then the tool succeeds
    And the tool result is:
      """
| Name       | PrefabName         | Id  | Connections | Logic | Room | Position      |
| ---------- | ------------------ | --- | ----------- | ----- | ---- | ------------- |
| Air Vent   | StructureAirVent   | 101 | p0:7        | yes   |    5 | 1.0, 2.0, 3.0 |
| Gas Sensor | StructureGasSensor | 102 | none        | yes   |    5 | 4.0, 5.0, 6.0 |
| Iron Pipe  | StructurePipe      | 103 | none        | no    | none | 7.0, 8.0, 9.0 |
      Showing 3 of 3.
      """

  Scenario: Search structures by name
    Given the world has structures
    When I call the "search_structures" tool with query "vent"
    Then the tool succeeds
    And the tool result is:
      """
| Name     | PrefabName       | Id  | Connections | Logic | Room | Position      |
| -------- | ---------------- | --- | ----------- | ----- | ---- | ------------- |
| Air Vent | StructureAirVent | 101 | p0:7        | yes   |    5 | 1.0, 2.0, 3.0 |
      Showing 1 of 1.
      """

  Scenario: Search one cable network
    Given the world has structures
    When I search structures on cable network "7"
    Then the tool succeeds
    And the search used cable network "7"

  Scenario: Search by name on one cable network
    Given the world has structures
    When I search structures for "vent" on cable network "7"
    Then the tool succeeds
    And the search used cable network "7"
    And the tool result is:
      """
| Name     | PrefabName       | Id  | Connections | Logic | Room | Position      |
| -------- | ---------------- | --- | ----------- | ----- | ---- | ------------- |
| Air Vent | StructureAirVent | 101 | p0:7        | yes   |    5 | 1.0, 2.0, 3.0 |
      Showing 1 of 1.
      """

  Scenario: Only logic structures
    Given the world has structures
    When I search structures with logic "true"
    Then the tool succeeds
    And the tool result is:
      """
| Name       | PrefabName         | Id  | Connections | Logic | Room | Position      |
| ---------- | ------------------ | --- | ----------- | ----- | ---- | ------------- |
| Air Vent   | StructureAirVent   | 101 | p0:7        | yes   |    5 | 1.0, 2.0, 3.0 |
| Gas Sensor | StructureGasSensor | 102 | none        | yes   |    5 | 4.0, 5.0, 6.0 |
      Showing 2 of 2.
      """

  Scenario: Only dumb objects
    Given the world has structures
    When I search structures with logic "false"
    Then the tool succeeds
    And the tool result is:
      """
| Name      | PrefabName    | Id  | Connections | Logic | Room | Position      |
| --------- | ------------- | --- | ----------- | ----- | ---- | ------------- |
| Iron Pipe | StructurePipe | 103 | none        | no    | none | 7.0, 8.0, 9.0 |
      Showing 1 of 1.
      """

  Scenario: Search one room
    Given the world has structures
    When I search structures in room "5"
    Then the tool succeeds
    And the search used room "5"
    And the tool result is:
      """
| Name       | PrefabName         | Id  | Connections | Logic | Room | Position      |
| ---------- | ------------------ | --- | ----------- | ----- | ---- | ------------- |
| Air Vent   | StructureAirVent   | 101 | p0:7        | yes   |    5 | 1.0, 2.0, 3.0 |
| Gas Sensor | StructureGasSensor | 102 | none        | yes   |    5 | 4.0, 5.0, 6.0 |
      Showing 2 of 2.
      """

  Scenario: No match
    Given the world has structures
    When I call the "search_structures" tool with query "nope"
    Then the tool succeeds
    And the tool result is:
      """
      No structures found for 'nope'.
      """

  Scenario: A logic filter must be true or false
    Given the world has structures
    When I search structures with logic "maybe"
    Then the tool fails with "Logic 'maybe' is not true or false."

  Scenario: A room filter must be a number
    Given the world has structures
    When I search structures in room "abc"
    Then the tool fails with "Room 'abc' is not a number."

  Scenario: A cable network filter must be a number
    Given the world has structures
    When I search structures on cable network "abc"
    Then the tool fails with "PowerNetworkId 'abc' is not a number."

  Scenario: Blank search states cap and total
    Given the world has 12 structures
    When I call the "search_structures" tool
    Then the tool succeeds
    And the tool result is:
      """
| Name         | PrefabName    | Id  | Connections | Logic | Room | Position       |
| ------------ | ------------- | --- | ----------- | ----- | ---- | -------------- |
| Structure 01 | StructurePipe | 101 | none        | no    | none | 1.0, 0.0, 0.0  |
| Structure 02 | StructurePipe | 102 | none        | no    | none | 2.0, 0.0, 0.0  |
| Structure 03 | StructurePipe | 103 | none        | no    | none | 3.0, 0.0, 0.0  |
| Structure 04 | StructurePipe | 104 | none        | no    | none | 4.0, 0.0, 0.0  |
| Structure 05 | StructurePipe | 105 | none        | no    | none | 5.0, 0.0, 0.0  |
| Structure 06 | StructurePipe | 106 | none        | no    | none | 6.0, 0.0, 0.0  |
| Structure 07 | StructurePipe | 107 | none        | no    | none | 7.0, 0.0, 0.0  |
| Structure 08 | StructurePipe | 108 | none        | no    | none | 8.0, 0.0, 0.0  |
| Structure 09 | StructurePipe | 109 | none        | no    | none | 9.0, 0.0, 0.0  |
| Structure 10 | StructurePipe | 110 | none        | no    | none | 10.0, 0.0, 0.0 |
      Showing 10 of 12.
      """

  Scenario: Logic structures sort first
    Given the world has cables plus a heater
    When I call the "search_structures" tool
    Then the tool succeeds
    And the tool result is:
      """
| Name        | PrefabName          | Id   | Connections | Logic | Room | Position      |
| ----------- | ------------------- | ---- | ----------- | ----- | ---- | ------------- |
| Wall Heater | StructureWallHeater | 1208 | p0:1194     | yes   |    5 | 1.0, 2.0, 3.0 |
| Cable 01    | StructureCable      |  201 | none        | no    | none | 1.0, 0.0, 0.0 |
| Cable 02    | StructureCable      |  202 | none        | no    | none | 2.0, 0.0, 0.0 |
| Cable 03    | StructureCable      |  203 | none        | no    | none | 3.0, 0.0, 0.0 |
| Cable 04    | StructureCable      |  204 | none        | no    | none | 4.0, 0.0, 0.0 |
| Cable 05    | StructureCable      |  205 | none        | no    | none | 5.0, 0.0, 0.0 |
| Cable 06    | StructureCable      |  206 | none        | no    | none | 6.0, 0.0, 0.0 |
| Cable 07    | StructureCable      |  207 | none        | no    | none | 7.0, 0.0, 0.0 |
| Cable 08    | StructureCable      |  208 | none        | no    | none | 8.0, 0.0, 0.0 |
| Cable 09    | StructureCable      |  209 | none        | no    | none | 9.0, 0.0, 0.0 |
      Showing 10 of 12.
      """

  Scenario: Offset pages past the cap
    Given the world has 12 structures
    When I search structures with offset "10"
    Then the tool succeeds
    And the tool result is:
      """
| Name         | PrefabName    | Id  | Connections | Logic | Room | Position       |
| ------------ | ------------- | --- | ----------- | ----- | ---- | -------------- |
| Structure 11 | StructurePipe | 111 | none        | no    | none | 11.0, 0.0, 0.0 |
| Structure 12 | StructurePipe | 112 | none        | no    | none | 12.0, 0.0, 0.0 |
      Showing 2 of 12.
      """

  Scenario: An offset must be a number
    Given the world has 12 structures
    When I search structures with offset "far"
    Then the tool fails with "Offset 'far' is not a number."

  Scenario: Structures with many ports list every connection
    Given the world has a battery and a furnace
    When I call the "search_structures" tool
    Then the tool succeeds
    And the tool result is:
      """
| Name            | PrefabName       | Id    | Connections                   | Logic | Room | Position      |
| --------------- | ---------------- | ----- | ----------------------------- | ----- | ---- | ------------- |
| Station Battery | StructureBattery | 10040 | p0:29457 p1:29457 d0:7        | yes   |    2 | 1.0, 2.0, 3.0 |
| Furnace         | StructureFurnace |  5229 | input:11 output:22 output2:33 | no    | none | 4.0, 5.0, 6.0 |
      Showing 2 of 2.
      """

  Scenario: Logic filter raises the cap
    Given the world has 12 logic structures
    When I search structures with logic "true"
    Then the tool succeeds
    And the tool found 12 structures
