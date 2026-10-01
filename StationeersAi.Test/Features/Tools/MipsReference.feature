Feature: MIPS reference tool

  The mips_reference tool looks up IC10 instructions from the game's editor
  help: usage and what each one does.

  Scenario: List every instruction
    Given the MIPS reference knows "l" and "yield"
    When I call the "mips_reference" tool
    Then the tool succeeds
    And the tool result is:
      """
      l: l r? device var
      yield: yield
      """

  Scenario: Describe one instruction
    Given the MIPS reference knows "l" and "yield"
    When I describe the instruction "l"
    Then the tool succeeds
    And the tool result is:
      """
      l: l r? device var
      Does l.
      """

  Scenario: Unknown instruction
    Given the MIPS reference knows "l" and "yield"
    When I describe the instruction "nope"
    Then the tool fails with "Unknown MIPS instruction 'nope'."
