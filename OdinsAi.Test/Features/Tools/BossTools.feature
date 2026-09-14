Feature: Boss tool

  The list_bosses tool reports which bosses have been slain.

  Background:
    Given the bosses are:
      | Boss      | Defeated |
      | Eikthyr   | Yes      |
      | The Elder | No       |
      | Bonemass  | Yes      |

  Scenario: List bosses and their defeat status
    When I call the "list_bosses" tool
    Then the tool succeeds
    And the boss result is:
      | Boss      | Defeated |
      | Bonemass  | Yes      |
      | Eikthyr   | Yes      |
      | The Elder | No       |

  Scenario: The boss status is returned as CSV
    When I call the "list_bosses" tool
    Then the tool succeeds
    And the tool result contains:
      """
      Boss,Defeated
      Bonemass,Yes
      Eikthyr,Yes
      The Elder,No
      """