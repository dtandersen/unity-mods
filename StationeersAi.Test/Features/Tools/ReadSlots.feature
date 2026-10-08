Feature: Read slots tool

  The read_slots tool lists what one thing holds, slot by slot: belts,
  crates, held tools. It reads only.

  Scenario: List a belt's slots
    Given the "Mining Belt" holds:
      | Slot   | Item       | Quantity |
      | Slot 1 | Iron Ore   | x12      |
      | Slot 2 | Copper Ore | x4       |
    When I read slots of thing "Mining Belt"
    Then the tool succeeds
    And the tool result is:
      """
      | Slot   | Name       | Quantity |
      | ------ | ---------- | -------- |
      | Slot 1 | Iron Ore   | x12      |
      | Slot 2 | Copper Ore | x4       |
      """

  Scenario: Thing with no slots reads empty
    Given nothing is in any slot
    When I read slots of thing "Crate"
    Then the tool succeeds
    And the tool result is:
      """
      Thing 'Crate' has no slots to read.
      """

  Scenario: Unknown thing
    Given no thing holds a match
    When I read slots of thing "Nope"
    Then the tool fails with "Thing 'Nope' not found."
