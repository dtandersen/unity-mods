Feature: Export IC tool

  The export_ic tool copies a computer's editor text onto a housing chip
  and verifies the burn by code hash. Sage only offers this when the
  stationeer agrees.

  Scenario: Export and verify
    Given the export succeeds
    When I export from computer "Lab Computer" to "Climate Control"
    Then the tool succeeds
    And the tool result is:
      """
      Exported 37 lines to Climate Control. Chip hash matches the editor.
      """

  Scenario: Chip reports a compile error
    Given the export fails with "Chip reports IncorrectVariable at line 12."
    When I export from computer "Lab Computer" to "Climate Control"
    Then the tool fails with "Chip reports IncorrectVariable at line 12."
