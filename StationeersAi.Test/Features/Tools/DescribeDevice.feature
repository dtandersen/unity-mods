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

  Scenario: Unknown device
    Given no device matches
    When I describe the device "Nope"
    Then the tool fails with "No single device matches 'Nope'."
