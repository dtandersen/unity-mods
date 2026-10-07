Feature: Write IC tool

  The write_ic tool places MIPS source in a computer or laptop's editor so the
  stationeer can review and Export it. The call names the hash of the chip
  under that computer's dropdown, so a program cannot be overwritten while the
  editor points at some other chip. It never writes to housings or chips.

  Scenario: Write code to a computer
    Given a computer "Lab Computer"
    And the chip under the dropdown holds "yield"
    When I write to computer "1189" with chip hash "6c96f2ae":
      """
      alias Device d0
      yield
      """
    Then the tool succeeds
    And the tool result is:
      """
      Wrote 2 lines to Lab Computer. The stationeer can review it in the editor and press Export.
      """

  Scenario: Write refuses a hash from another chip
    Given a computer "Lab Computer"
    And the chip under the dropdown holds "yield"
    When I write to computer "1189" with chip hash "811c9dc5":
      """
      yield
      """
    Then the tool fails with "Chip on Climate Control holds 6c96f2ae, not 811c9dc5. Read it, then write with its hash."

  Scenario: Write refuses when the dropdown points at nothing
    Given a computer "Lab Computer"
    And the computer points at nothing
    When I write to computer "1189" with chip hash "6c96f2ae":
      """
      yield
      """
    Then the tool fails with "The computer points at no housing. Pick the housing first."

  Scenario: Write needs a chip hash
    Given a computer "Lab Computer"
    When I write to computer "1189" without a chip hash:
      """
      yield
      """
    Then the tool fails with "Missing required argument 'hash'. Read the chip with read_ic and pass its hash."

  Scenario: Write code by label is refused
    Given a computer "Lab Computer"
    And the chip under the dropdown holds "yield"
    When I write to computer "Lab Computer" with chip hash "6c96f2ae":
      """
      yield
      """
    Then the tool fails with "'computer' is not a thing id: 'Lab Computer'. Read it from search_devices."

  Scenario: Unknown computer
    Given no computer is found
    When I write to computer "999" with chip hash "6c96f2ae":
      """
      yield
      """
    Then the tool fails with "Computer 999 not found."
