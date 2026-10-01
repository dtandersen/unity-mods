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
      | Pressure | 101.3 | no       |
      | On       |     1 | yes      |
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
      | Pressure | 101.3 | no       |
      | On       |     1 | yes      |
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
