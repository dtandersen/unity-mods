Feature: Room tools

  The read_room tool reads a room's atmospherics so the assistant can judge
  air it cannot see: temperature, pressure, and gas moles. A room id comes
  from the Room column of search_structures; omitting it reads the room the
  player stands in.

  Background:
    Given a Bedroom room 5 at 21.5 C and 101.3 kPa in 12500 L holding:
      | MoleType | Moles |
      | Oxygen   | 2500  |
      | Nitrogen | 9400  |
      | Steam    | 120   |
    And I stand in room 5

  Scenario: Read a room by id
    When I call the "read_room" tool with room "5"
    Then the tool succeeds
    And the tool result is:
      """
      Room 5
      Temperature: 21.5°C
      Pressure: 101.3 kPa
      Volume: 12500 L
      Gas:
      Oxygen: 2500 mol
      Nitrogen: 9400 mol
      Steam: 120 mol
      """

  Scenario: Liquids list under Liquid
    Given a Bedroom room 6 at 20 C and 100 kPa in 8000 L holding:
      | MoleType | Moles |
      | Water | 300   |
    When I call the "read_room" tool with room "6"
    Then the tool succeeds
    And the tool result is:
      """
      Room 6
      Temperature: 20°C
      Pressure: 100 kPa
      Volume: 8000 L
      Liquid:
      Water: 300 mol
      """

  Scenario: A vacuum room reads without crashing
    Given a Bedroom room 7 at -50 C and 0 kPa in 8000 L holding:
      | MoleType | Moles |
    When I call the "read_room" tool with room "7"
    Then the tool succeeds
    And the tool result is:
      """
      Room 7
      Temperature: -50°C
      Pressure: 0 kPa
      Volume: 8000 L
      Vacuum
      """

  Scenario: An omitted room reads the player's room
    When I call the "read_room" tool without arguments
    Then the tool succeeds
    And the tool result contains "Room 5"

  Scenario: An omitted room fails outside rooms
    Given I stand outside
    When I call the "read_room" tool without arguments
    Then the tool fails with "You are outside a room."

  Scenario: An unknown room fails
    When I call the "read_room" tool with room "9"
    Then the tool fails with "Room 9 was not found."

  Scenario: A room id must be a number
    When I call the "read_room" tool with room "abc"
    Then the tool fails with "Room 'abc' is not a number."
