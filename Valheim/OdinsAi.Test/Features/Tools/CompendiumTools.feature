Feature: Compendium tools

  The search_compendium and read_compendium tools expose the in-game compendium
  (lore and Munin's hints) so the assistant can answer questions about the world.

  Background:
    Given the compendium contains:
      | Topic         | Text                                                      |
      | Boar hunting  | Boars can be found in the meadows and tamed with carrots. |
      | Swamp dangers | Beware the draugr and the leeches in the swamps.          |

  Scenario: Search the compendium by topic
    When I call the "search_compendium" tool with query "boar"
    Then the tool succeeds
    And the compendium result topics are "Boar hunting"

  Scenario: Search the compendium by body text
    When I call the "search_compendium" tool with query "draugr"
    Then the tool succeeds
    And the compendium result topics are "Swamp dangers"

  Scenario: The compendium search requires its query argument
    When I call the "search_compendium" tool without arguments
    Then the tool fails with "Missing required argument 'query'."

  Scenario: Read a compendium page
    When I call the "read_compendium" tool with topic "Boar hunting"
    Then the tool succeeds
    And the compendium page "Boar hunting" reads:
      """
      Boars can be found in the meadows and tamed with carrots.
      """

  Scenario: Reading an unknown topic fails
    When I call the "read_compendium" tool with topic "Dragons"
    Then the tool fails with "No compendium page found for 'Dragons'. Use search_compendium first."