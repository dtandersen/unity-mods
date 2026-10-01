Feature: Write IC tool

  The write_ic tool places MIPS source in a computer or laptop's editor so the
  stationeer can review and Export it. It never writes to housings or chips.

  Scenario: Write code to a computer
    Given a computer "Lab Computer"
    When I write to device "Lab Computer":
      """
      alias Device d0
      yield
      """
    Then the tool succeeds
    And the tool result contains "Wrote 2 lines to Lab Computer"

  Scenario: Unknown computer
    Given no computer is found
    When I write to device "Nope":
      """
      yield
      """
    Then the tool fails with "Computer 'Nope' not found."
