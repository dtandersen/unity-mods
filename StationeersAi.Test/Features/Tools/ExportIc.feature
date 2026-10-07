Feature: Export IC tool

  The export_ic tool copies a computer's editor text onto a housing chip
  and verifies the burn by code hash. Sage only offers this when the
  stationeer agrees.

  Scenario: Export and verify
    Given the export succeeds
    When I export from computer "1189" to "2200"
    Then the tool succeeds
    And the tool result is:
      """
      Exported 37 lines to Climate Control. Chip hash matches the editor.
      """

  Scenario: Chip reports a compile error
    Given the export fails with "Chip reports IncorrectVariable at line 12."
    When I export from computer "1189" to "2200"
    Then the tool fails with "Chip reports IncorrectVariable at line 12."

  Scenario: Export names the housing by label
    When I export from computer "1189" to "Climate Control"
    Then the tool fails with "'device' is not a thing id: 'Climate Control'. Read it from search_devices."

  Scenario: Import pulls chip into editor
    Given the import succeeds
    When I import from housing "2200" into "1189"
    Then the tool succeeds
    And the tool result is:
      """
      Pulled 3 lines from housing 2200 into Lab Computer.
      """

  Scenario: Import fails when the housing is unknown
    Given the import fails with "Housing 999 not found on this computer's network. Available: none"
    When I import from housing "999" into "1189"
    Then the tool fails with "Housing 999 not found on this computer's network. Available: none"
