Feature: Pipe network tools

  The read_pipenetwork tool reads a pipe network's air so the assistant can
  judge what flows through pipes: pressure, temperature, and gas moles.
  A pipe network id comes from describe_structure; a device label or id finds the
  pipe networks joined to it. Data networks (cables) are separate.

  Background:
    Given a Liquid pipe network 21665 at 20 C and 100 kPa in 100 L holding:
      | MoleType | Moles |
      | Water    | 300   |
      | Oxygen   | 10    |
    And device "20211" joins pipe network 21665

  Scenario: Read a pipe network by id
    When I call the "read_pipenetwork" tool with pipenetwork "21665"
    Then the tool succeeds
    And the tool result is:
      """
      Pipe network 21665 (Liquid)
      Temperature: 20°C
      Pressure: 100 kPa
      Volume: 100 L
      Total: 310 mol
      Gas:
      Oxygen: 10 mol
      Liquid:
      Water: 300 mol
      Members (1):
      Liquid Pipe Straight #21798
      """

  Scenario: Read pipe networks joined to a device
    When I call the "read_pipenetwork" tool with device "20211"
    Then the tool succeeds
    And the tool result contains "Pipe network 21665"

  Scenario: A vacuum pipe network reads without crashing
    Given a Gas pipe network 21666 at -50 C and 0 kPa in 50 L holding:
      | MoleType | Moles |
    When I call the "read_pipenetwork" tool with pipenetwork "21666"
    Then the tool succeeds
    And the tool result contains "Vacuum"

  Scenario: An unknown pipe network fails
    When I call the "read_pipenetwork" tool with pipenetwork "9"
    Then the tool fails with "Pipe network 9 was not found."

  Scenario: A pipe network id must be a number
    When I call the "read_pipenetwork" tool with pipenetwork "abc"
    Then the tool fails with "Pipe network 'abc' is not a number."

  Scenario: Missing args fail
    When I call the "read_pipenetwork" tool without arguments
    Then the tool fails with "Missing required argument 'pipenetwork' or 'device'."
