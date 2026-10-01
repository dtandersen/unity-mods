Feature: Write IC tool

  The write_ic tool places MIPS source in a computer or laptop's editor so the
  stationeer can review and Export it. It never writes to housings or chips.

  Scenario: Write code to a computer
    Given a computer "Lab Computer"
    When I write to computer "Lab Computer":
      """
      alias Device d0
      yield
      """
    Then the tool succeeds
    And the tool result is:
      """
      Wrote 2 lines to Lab Computer. The stationeer can review it in the editor and press Export.
      """

  Scenario: Write code by thing id
    Given a computer "Lab Computer"
    When I write to thing "1189":
      """
      yield
      """
    Then the tool succeeds
    And the tool result is:
      """
      Wrote 1 line to Lab Computer. The stationeer can review it in the editor and press Export.
      """

  Scenario: Unknown computer
    Given no computer is found
    When I write to computer "Nope":
      """
      yield
      """
    Then the tool fails with "Computer 'Nope' not found."
