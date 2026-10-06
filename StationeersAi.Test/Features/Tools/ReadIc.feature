Feature: Read IC tool

  The read_ic tool returns the full text in a computer or laptop's editor,
  so Sage can review it before overwriting or pick up after a reload.
  With a housing device it also returns the burned chip, without touching
  the editor. It never writes.

  Scenario: Read the editor
    Given the editor holds "yield"
    When I read computer "Lab Computer"
    Then the tool succeeds
    And the tool result is:
      """
      yield
      Code hash: 6c96f2ae
      """

  Scenario: Empty editor
    Given the editor is empty
    When I read computer "Lab Computer"
    Then the tool succeeds
    And the tool result is:
      """
      (editor is empty)
      """

  Scenario: Read by thing id
    Given the editor holds "yield"
    When I read computer "1189"
    Then the tool succeeds
    And the tool result is:
      """
      yield
      Code hash: 6c96f2ae
      """

  Scenario: Unknown computer
    Given no computer is found
    When I read computer "Nope"
    Then the tool fails with "Computer 'Nope' not found."

  Scenario: Read the housing chip
    Given the editor holds "yield"
    And the housing holds "move r0 1" on "Climate Control"
    When I read computer "Lab Computer" housing "Climate Control"
    Then the tool succeeds
    And the tool result is:
      """
      yield
      Code hash: 6c96f2ae

      Housing Climate Control chip:
      move r0 1
      Code hash: c26e09e1
      """

  Scenario: Unknown housing
    Given the editor holds "yield"
    And no housing is found
    When I read computer "Lab Computer" housing "Nope"
    Then the tool fails with "Housing 'Nope' not found."
