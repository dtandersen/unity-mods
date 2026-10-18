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
    Given the chip reports "IncorrectVariable at line 12"
    When I export from computer "1189" to "2200"
    Then the tool fails with "Chip reports IncorrectVariable at line 12."

  Scenario: A burn that silently fails
    Given the burn does not take
    When I export from computer "1189" to "2200"
    Then the tool fails with "Chip hash does not match the editor."

  Scenario: Export names the housing by label
    When I export from computer "1189" to "Climate Control"
    Then the tool fails with "'device' is not a thing id: 'Climate Control'. Read it from search_devices."

  Scenario: Export to a housing off the network
    Given the export succeeds
    And no housing is found
    When I export from computer "1189" to "999"
    Then the tool fails with "Housing 999 not found on this computer's network. Available: Climate Control #2200"

  Scenario: Export needs a housing
    When I export from computer "1189" without a housing
    Then the tool fails with "Missing required argument 'device'."
