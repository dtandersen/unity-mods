Feature: Sage presenter

  The presenter drives the Sage window's passive view. Submitting sends the
  typed question and marks the window busy; a busy window takes no second
  question until the answer finishes; answers reach the view; the toggle key
  shows and hides the window.

  Scenario: Submit sends the typed question
    Given the window input is "how do I make cable?"
    When the user submits
    Then the asked question is "how do I make cable?"
    And the window input is ""
    And the window is busy
    And the view is busy

  Scenario: Empty input submits nothing
    Given the window input is "   "
    When the user submits
    Then nothing is asked
    And the window is not busy
    And the view is not busy

  Scenario: A busy window accepts no question
    Given the assistant is still answering "first question"
    And the window input is "second question"
    When the user submits
    Then the asked question is "first question"
    And the window input is "second question"

  Scenario: The window leaves busy when the answer finishes
    Given the assistant is still answering "hello"
    When the answer finishes
    Then the window is not busy
    And the view is not busy

  Scenario: An external question runs a turn
    When the console asks "how do I make cable?"
    Then the asked question is "how do I make cable?"
    And the window is busy
    And the view is busy
    When the answer finishes
    Then the window is not busy
    And the view is not busy

  Scenario: An external question is ignored while busy
    Given the assistant is still answering "first question"
    When the console asks "second question"
    Then the asked question is "first question"

  Scenario: Answers reach the view
    When the presenter receives the line "Sage: hi"
    Then the view shows:
      """
      Sage: hi
      """

  Scenario: The view holds the newest lines
    Given the view history holds 3 lines
    When the presenter receives the line "one"
    And the presenter receives the line "two"
    And the presenter receives the line "three"
    And the presenter receives the line "four"
    Then the view shows:
      """
      two
      three
      four
      """

  Scenario: Toggling shows and hides
    When the user toggles the window
    Then the window is visible
    And the input is focused
    When the user toggles the window
    Then the window is hidden
