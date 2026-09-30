Feature: Structure tools

  The search_structures tool looks up buildable structures so the assistant can
  answer "what do I build for X" questions.

  Background:
    Given the structure database contains:
      | Name               | Description       |
      | Solar Panel        | Generates power.  |
      | Solid Fuel Generator | Burns coal.     |

  Scenario: Look up a structure by name
    When I call the "search_structures" tool with query "solar"
    Then the tool succeeds
    And the structure result is:
      | Name        | Description      |
      | Solar Panel | Generates power. |

  Scenario: The structure tool requires its query argument
    When I call the "search_structures" tool without arguments
    Then the tool fails with "Missing required argument 'query'."

  Scenario: No match reports the query
    When I call the "search_structures" tool with query "airlock"
    Then the tool succeeds
    And the tool result contains "No structures found for 'airlock'."
