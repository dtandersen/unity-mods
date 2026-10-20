Feature: Describe structure tool

  The describe_structure tool reports one live structure in full: its logic
  settings with live values, its slots with occupants, and how far its
  construction has come.

  Scenario: Describe a structure
    Given the structure "Air Vent" exists
    When I describe the structure "Air Vent"
    Then the tool succeeds
    And the tool result is:
      """
      Structure: Air Vent (StructureAirVent #101)
      Connections: p0:7, On: yes
      Build states: unknown.
      Logic:
| Name     | Value | Writable |
| -------- | ----- | -------- |
| On       |     1 | yes      |
| Pressure | 101.3 | no       |
      Slots:
| # | SlotType | Occupant |
| - | -------- | -------- |
| 0 | Filter   | empty    |
      """

  Scenario: Describe by reference id
    Given the structure "Air Vent" exists
    When I describe the structure "101"
    Then the tool succeeds
    And the tool result is:
      """
      Structure: Air Vent (StructureAirVent #101)
      Connections: p0:7, On: yes
      Build states: unknown.
      Logic:
| Name     | Value | Writable |
| -------- | ----- | -------- |
| On       |     1 | yes      |
| Pressure | 101.3 | no       |
      Slots:
| # | SlotType | Occupant |
| - | -------- | -------- |
| 0 | Filter   | empty    |
      """

  Scenario: Describe an unfinished structure
    Given the structure "Furnace" stands at stage 1 of 3 with steps:
      | State | PrefabName       | Name          | Quantity |
      | 2     | ItemWrench       | Wrench        |          |
      | 2     | ItemIronSheets   | Iron Sheets   | 2        |
      | 3     | ItemWeldingTorch | Welding Torch |          |
      | 3     | ItemIronSheets   | Iron Sheets   | 2        |
    When I describe the structure "Furnace"
    Then the tool succeeds
    And the tool result is:
      """
      Structure: Furnace (StructureFurnace #101)
      Connections: p0:7, On: yes
      Build states: 1/3.
      2. Wrench - Iron Sheets x2
      3. Welding Torch - Iron Sheets x2
      Logic: none readable.
      Slots: none.
      """

  Scenario: Describe a finished structure
    Given the structure "Air Vent" stands finished
    When I describe the structure "Air Vent"
    Then the tool succeeds
    And the tool result is:
      """
      Structure: Air Vent (StructureAirVent #101)
      Connections: p0:7, On: yes
      Build states: complete.
      Logic: none readable.
      Slots: none.
      """

  Scenario: Describe a structure needing tools
    Given the structure "Satellite Dish" stands at stage 1 of 4 with steps:
      | State | PrefabName      | Name         | Quantity |
      | 2     | ItemWrench      | Wrench       |          |
      | 2     | ItemSteelSheets | Steel Sheets | 20       |
      | 3     | ItemCableCoil   | Cable Coil   | 10       |
      | 4     | ItemWrench      | Wrench       |          |
    When I describe the structure "Satellite Dish"
    Then the tool succeeds
    And the tool result is:
      """
      Structure: Satellite Dish (StructureSatelliteDish #101)
      Connections: p0:7, On: yes
      Build states: 1/4.
      2. Wrench - Steel Sheets x20
      3. Cable Coil x10
      4. Wrench
      Logic: none readable.
      Slots: none.
      """

  Scenario: Describe needs a structure
    When I call the "describe_structure" tool without arguments
    Then the tool fails with "Missing required argument 'structure'."

  Scenario: Describe a structure holding reagents
    Given the structure "Autolathe" holds:
      | Reagent | Quantity |
      | Iron    | 60       |
    When I describe the structure "Autolathe"
    Then the tool succeeds
    And the tool result is:
      """
      Structure: Autolathe (StructureAutolathe #1456)
      Connections: none, On: no
      Build states: unknown.
      Logic: none readable.
      Slots:
| # | SlotType | Occupant |
| - | -------- | -------- |
| 0 | Import   | empty    |
| 1 | Export   | empty    |
      Storage:
| Reagent | Quantity |
| ------- | -------- |
| Iron    |       60 |
      """

  Scenario: Describe a structure with empty storage
    Given the structure "Autolathe" holds:
      | Reagent | Quantity |
    When I describe the structure "Autolathe"
    Then the tool succeeds
    And the tool result contains "Storage: empty."

  Scenario: Describe a housing
    Given the housing "Housing" exists
    When I describe the structure "Housing"
    Then the tool succeeds
    And the tool result is:
      """
      Structure: Housing (StructureCircuitHousing #100)
      Connections: p0:7
      Build states: unknown.
      Logic: none readable.
      Slots: none.
      Structures:
| Index | Label   | Id           |
| ----- | ------- | ------------ |
| db    | Housing | Housing #100 |
| d0    | Heater  | Heater #101  |
| d1    | empty   | disconnected |
      """

  Scenario: Housing chip state
    Given the housing "Housing" runs code
    When I describe the structure "Housing"
    Then the tool succeeds
    And the tool result is:
      """
      Structure: Housing (StructureCircuitHousing #100)
      Connections: p0:7
      Build states: unknown.
      Logic: none readable.
      Slots: none.
      Structures:
| Index | Label   | Id           |
| ----- | ------- | ------------ |
| db    | Housing | Housing #100 |
| d0    | Heater  | Heater #101  |
| d1    | empty   | disconnected |
      Chip:
      Error: IncorrectVariable at line 12
      Display: 294
      Code hash: 6c96f2ae
      """

  Scenario: Unknown structure
    Given no structure matches
    When I describe the structure "Nope"
    Then the tool fails with "No single structure matches 'Nope'."

  Scenario: Zero ratios are dropped by default
    Given the gas sensor "Gas Sensor" reads 290K at 3.3 kPa
    When I describe the structure "Gas Sensor"
    Then the tool succeeds
    And the tool result is:
      """
      Structure: Gas Sensor (StructureGasSensor #1482)
      Connections: p0:1194
      Build states: unknown.
      Logic:
| Name               | Value     | Writable |
| ------------------ | --------- | -------- |
| Temperature        |   290.131 | no       |
| Pressure           |   3.35518 | no       |
| RatioCarbonDioxide |  0.774701 | no       |
| RatioNitrogen      | 0.0241535 | no       |
| RatioOxygen        |  0.195957 | no       |
      Slots: none.
      """

  Scenario: Full list returns on ask
    Given the gas sensor "Gas Sensor" reads 290K at 3.3 kPa
    When I describe the structure "Gas Sensor" with full "true"
    Then the tool succeeds
    And the tool result is:
      """
      Structure: Gas Sensor (StructureGasSensor #1482)
      Connections: p0:1194
      Build states: unknown.
      Logic:
| Name               | Value     | Writable |
| ------------------ | --------- | -------- |
| Temperature        |   290.131 | no       |
| Pressure           |   3.35518 | no       |
| RatioCarbonDioxide |  0.774701 | no       |
| RatioMethane       |         0 | no       |
| RatioNitrogen      | 0.0241535 | no       |
| RatioOxygen        |  0.195957 | no       |
| RatioWater         |         0 | no       |
      Slots: none.
      """

  Scenario: Two sensors with one name list both matches
    Given two structures share the name "Gas Sensor"
    When I describe the structure "Gas Sensor"
    Then the tool fails with:
      """
      No single structure matches 'Gas Sensor'. Matches:
| Name       | PrefabName         | Id   | Connections |
| ---------- | ------------------ | ---- | ----------- |
| Gas Sensor | StructureGasSensor | 1482 | p0:1194     |
| Gas Sensor | StructureGasSensor | 1123 | p0:1119     |
      """
