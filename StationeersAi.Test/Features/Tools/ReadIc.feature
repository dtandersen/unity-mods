Feature: Read IC tool

  The read_ic tool returns the full text in a computer or laptop's editor,
  so Sage can review it before overwriting or pick up after a reload.

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
