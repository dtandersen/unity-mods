Feature: Describe thing tool

  The describe_thing tool reports one live thing in full: a built
  structure returns its structure values, a loose item returns quantity
  and position.

  Scenario: Describe a structure on a cable network
    Given the structure thing "Air Vent" exists
    When I describe the thing "Air Vent"
    Then the tool succeeds
    And the tool result is:
      """
      Thing: Air Vent (StructureAirVent #101) [structure]
      Quantity: none
      Position: 1.0, 2.0, 3.0, Room: 5
      Connections: p0:7, Logic: yes
      On: yes
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
    Given the structure thing "Air Vent" exists
    When I describe the thing "101"
    Then the tool succeeds
    And the tool result is:
      """
      Thing: Air Vent (StructureAirVent #101) [structure]
      Quantity: none
      Position: 1.0, 2.0, 3.0, Room: 5
      Connections: p0:7, Logic: yes
      On: yes
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

  Scenario: Describe a loose thing
    Given the loose thing "Iron Ore" exists
    When I describe the thing "Iron Ore"
    Then the tool succeeds
    And the tool result is:
      """
      Thing: Iron Ore (OreIron #201) [thing]
      Quantity: x5
      Position: 4.0, 5.0, 6.0, Room: none
      Connections: none, Logic: no
      Structure details: none (loose thing).
      """

  Scenario: The thing tool requires its thing argument
    When I describe the thing ""
    Then the tool fails with "Missing required argument 'thing'."

  Scenario: Unknown thing
    Given no thing matches
    When I describe the thing "Nope"
    Then the tool fails with "No single thing matches 'Nope'."
