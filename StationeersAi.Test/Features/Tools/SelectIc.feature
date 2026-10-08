Feature: Select IC tool

  The select_ic tool moves a computer's dropdown to one housing on its
  network. Nothing burns and nothing moves: the selection only changes
  which chip the next guarded write checks its hash against.

  Scenario: Point the dropdown
    When I point computer "1189" at housing "2200"
    Then the tool succeeds
    And the tool result is:
      """
      The dropdown now points at Climate Control.
      """

  Scenario: Point at a housing off the network
    Given no housing is found
    When I point computer "1189" at housing "999"
    Then the tool fails with "Housing 999 not found on this computer's network. Available: Climate Control #2200"

  Scenario: Point names the housing by label
    When I point computer "1189" at housing "Climate Control"
    Then the tool fails with "'device' is not a thing id: 'Climate Control'. Read it from search_devices."
