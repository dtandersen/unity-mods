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

  Scenario: Import pulls chip into editor
    Given the import succeeds
    When I import from housing "Climate Control" into "Lab Computer"
    Then the tool succeeds
    And the tool result is:
      """
      Pulled 3 lines from Climate Control into Lab Computer.
      """

  Scenario: Import fails when the housing is unknown
    Given the import fails with "Housing 'Nope' not found on this computer's network. Available: none"
    When I import from housing "Nope" into "Lab Computer"
    Then the tool fails with "Housing 'Nope' not found on this computer's network. Available: none"
