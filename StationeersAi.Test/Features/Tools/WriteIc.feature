Feature: Write IC tool

  The write_ic tool places MIPS source in a computer or laptop's editor so the
  stationeer can review and Export it. It never writes to housings or chips.

  Scenario: Write code to a computer
    Given a computer "Lab Computer"
    When I write to computer "1189":
      """
      alias Device d0
      yield
      """
    Then the tool succeeds
    And the tool result is:
      """
      Wrote 2 lines to Lab Computer. The stationeer can review it in the editor and press Export.
      """

  Scenario: Write code by label is refused
    Given a computer "Lab Computer"
    When I write to computer "Lab Computer":
      """
      yield
      """
    Then the tool fails with "'computer' is not a thing id: 'Lab Computer'. Read it from search_devices."

  Scenario: Unknown computer
    Given no computer is found
    When I write to computer "999":
      """
      yield
      """
    Then the tool fails with "Computer 999 not found."
