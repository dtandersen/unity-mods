Feature: Compass directions

  A direction vector maps to an 8-way compass point. In Valheim, +Z is north and
  +X is east.

  Scenario Outline: Vectors map to compass points
    When I compute the compass direction <dx>, <dz>
    Then the compass direction is "<point>"

    Examples:
      | dx | dz | point |
      | 0  | 1  | N     |
      | 1  | 0  | E     |
      | 0  | -1 | S     |
      | -1 | 0  | W     |
      | 1  | 1  | NE    |
      | -1 | 1  | NW    |
      | 1  | -1 | SE    |
      | -1 | -1 | SW    |