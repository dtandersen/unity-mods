Feature: Describe device tool

  The describe_device tool reports one live device in full: its logic
  settings with live values, and its slots with occupants.

  Scenario: Describe a device
    Given the device "Air Vent" exists
    When I describe the device "Air Vent"
    Then the tool succeeds
    And the tool result is:
      """
      Device: Air Vent (StructureAirVent #101)
      Network: 7, On: yes
      Logic:
      | Name     | Value | Writable |
      | -------- | ----- | -------- |
      | On       |     1 | yes      |
      | Pressure | 101.3 | no       |
      Slots:
      | # | Slot   | Occupant |
      | - | ------ | -------- |
      | 0 | Filter | empty    |
      """

  Scenario: Describe by reference id
    Given the device "Air Vent" exists
    When I describe the device "101"
    Then the tool succeeds
    And the tool result is:
      """
      Device: Air Vent (StructureAirVent #101)
      Network: 7, On: yes
      Logic:
      | Name     | Value | Writable |
      | -------- | ----- | -------- |
      | On       |     1 | yes      |
      | Pressure | 101.3 | no       |
      Slots:
      | # | Slot   | Occupant |
      | - | ------ | -------- |
      | 0 | Filter | empty    |
      """

  Scenario: Describe a housing
    Given the housing "Housing" exists
    When I describe the device "Housing"
    Then the tool succeeds
    And the tool result is:
      """
      Device: Housing (StructureCircuitHousing #100)
      Network: 7
      Logic: none readable.
      Slots: none.
      Devices:
      | Index | Label   | Thing        |
      | ----- | ------- | ------------ |
      | db    | Housing | Housing #100 |
      | d0    | Heater  | Heater #101  |
      | d1    | empty   | disconnected |
      """

  Scenario: Housing chip state
    Given the housing "Housing" runs code
    When I describe the device "Housing"
    Then the tool succeeds
    And the tool result is:
      """
      Device: Housing (StructureCircuitHousing #100)
      Network: 7
      Logic: none readable.
      Slots: none.
      Devices:
      | Index | Label   | Thing        |
      | ----- | ------- | ------------ |
      | db    | Housing | Housing #100 |
      | d0    | Heater  | Heater #101  |
      | d1    | empty   | disconnected |
      Chip:
      Error: IncorrectVariable at line 12
      Display: 294
      Code hash: 6c96f2ae
      """

  Scenario: Unknown device
    Given no device matches
    When I describe the device "Nope"
    Then the tool fails with "No single device matches 'Nope'."

  Scenario: Zero ratios are dropped by default
    Given the gas sensor "Gas Sensor" reads 290K at 3.3 kPa
    When I describe the device "Gas Sensor"
    Then the tool succeeds
    And the tool result is:
      """
      Device: Gas Sensor (StructureGasSensor #1482)
      Network: 1194
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
    When I describe the device "Gas Sensor" with full "true"
    Then the tool succeeds
    And the tool result is:
      """
      Device: Gas Sensor (StructureGasSensor #1482)
      Network: 1194
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
    Given two devices share the name "Gas Sensor"
    When I describe the device "Gas Sensor"
    Then the tool fails with:
      """
      No single device matches 'Gas Sensor'. Matches:
      | Name       | Type               | Thing | Network |
      | ---------- | ------------------ | ----- | ------- |
      | Gas Sensor | StructureGasSensor |  1482 |    1194 |
      | Gas Sensor | StructureGasSensor |  1123 |    1119 |
      """
