Feature: Import IC tool

  The import_ic tool pulls the chip under a computer's dropdown into its
  editor so the stationeer can read and change it. The game's button takes
  no target: point the dropdown first with select_ic. It burns nothing;
  read_ic shows what landed on the screen.

  Scenario: Import pulls the selected chip into the editor
    Given the chip of housing "Climate Control" holds:
      """
      move r0 1
      wait
      yield
      """
    When I import into "1189"
    Then the tool succeeds
    And the tool result is:
      """
      Pulled 3 lines into Lab Computer.
      """

  Scenario: Import refuses when nothing is selected
    Given the computer points at nothing
    When I import into "1189"
    Then the tool fails with "The computer points at no housing. Pick the housing first."
