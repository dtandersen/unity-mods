Feature: Window model

  The window model is the Sage window's history store: appended lines, the
  newest kept past the cap. It decides nothing; the presenter drives the view.

  Scenario: Answers append to history
    When the model receives the line "Sage: hi"
    Then the window history is:
      """
      Sage: hi
      """

  Scenario: History holds the newest lines
    Given the window history holds 3 lines
    When the model receives the line "one"
    And the model receives the line "two"
    And the model receives the line "three"
    And the model receives the line "four"
    Then the window history is:
      """
      two
      three
      four
      """

  Scenario: Clearing empties the history
    Given the model received the line "stale"
    When the history is cleared
    Then the window history is empty
