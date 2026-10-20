Feature: Storage tools

  The search_storage tool searches containers within 100 metres and returns the
  total amount of each matching item across them, not each individual stack.

  Background:
    Given the nearby containers contain:
      | Container  | Item   | Amount | Type     |
      | Wood chest | Turnip | 20     | material |
      | Wood chest | Wood   | 50     | material |
      | Iron chest | Turnip | 5      | material |

  Scenario: Search totals matching items across nearby containers
    When I call the "search_storage" tool with query "turnip"
    Then the tool succeeds
    And the storage result is:
      | Name   | Amount |
      | Turnip | 25     |

  Scenario: The storage tool requires its query argument
    When I call the "search_storage" tool without arguments
    Then the tool fails with "Missing required argument 'query'."