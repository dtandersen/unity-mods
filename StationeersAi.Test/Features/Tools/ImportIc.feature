Feature: Import IC tool

  The import_ic tool pulls a housing chip's burned program into a
  computer's editor so the stationeer can read and change it. It burns
  nothing; read_ic shows what landed on the screen.

  Scenario: Import pulls chip into editor
    Given the chip of housing "Climate Control" holds:
      """
      move r0 1
      wait
      yield
      """
    When I import from housing "2200" into "1189"
    Then the tool succeeds
    And the tool result is:
      """
      Pulled 3 lines from housing 2200 into Lab Computer.
      """

  Scenario: Import names the housing by label
    When I import from housing "Climate Control" into "1189"
    Then the tool fails with "'device' is not a thing id: 'Climate Control'. Read it from search_devices."
