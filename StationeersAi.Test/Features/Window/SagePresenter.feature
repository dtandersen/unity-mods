Feature: Sage presenter

  The presenter drives the Sage window's passive view and runs each turn:
  submits and console questions echo into the window history, a busy window
  takes no second turn until the answer finishes, and the toggle key shows
  and hides the window.

  Scenario: Submit runs a turn
    Given the AI model will answer "hi there"
    And the window input is "how are you?"
    When the user submits
    And the turn completes
    Then the asked question is "how are you?"
    And the window input is ""
    And the window is not busy
    And the view is not busy
    And the view shows:
      """
      Tester: how are you?
      Sage: hi there
      """

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

  Scenario: The window leaves busy when the turn finishes
    Given the assistant is still answering "hello"
    When the answer finishes
    Then the window is not busy
    And the view is not busy

  Scenario: A console question runs a turn
    Given the AI model will answer "42"
    When the console asks "what is the answer"
    And the turn completes
    Then the asked question is "what is the answer"
    And the window is not busy
    And the view shows:
      """
      Tester: what is the answer
      Sage: 42
      """

  Scenario: A console question is ignored while busy
    Given the assistant is still answering "first question"
    When the console asks "second question"
    Then the asked question is "first question"

  Scenario: A missing player name falls back to You
    Given my player name is ""
    And the AI model will answer "hi"
    And the window input is "hello"
    When the user submits
    And the turn completes
    Then the view shows:
      """
      You: hello
      Sage: hi
      """

  Scenario: The assistant name is used
    Given the assistant is named "Ada"
    And the AI model will answer "hi"
    And the window input is "hello"
    When the user submits
    And the turn completes
    Then the view shows:
      """
      Tester: hello
      Ada: hi
      """

  Scenario: Missing game data is reported
    Given game data is not loaded
    And the window input is "hello"
    When the user submits
    And the turn completes
    Then the window is not busy
    And the view shows:
      """
      Tester: hello
      Sage: game data is not loaded yet.
      """

  Scenario: Reasoning is shown when thinking is on
    Given the AI model reasons "weigh the options" and answers "Go north"
    And thinking display is on
    And the window input is "which way"
    When the user submits
    And the turn completes
    Then the view shows:
      """
      Tester: which way
      Sage: weigh the options
      Sage: Go north
      """

  Scenario: Tool traffic stays hidden by default
    Given the AI model will request the tool "search_items" with query "cable"
    And the AI model will answer "done"
    And the window input is "how do I make cable?"
    When the user submits
    And the turn completes
    Then the view shows:
      """
      Tester: how do I make cable?
      Sage: done
      """

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

  Scenario: Clearing drops the context
    When the user clears the context
    Then the view shows:
      """
      Sage: context cleared (0 messages).
      """

  Scenario: Compacting summarizes the context
    Given the AI model will answer "First."
    And the window input is "first question"
    When the user submits
    And the turn completes
    Given the AI model will answer "Summary."
    When the user compacts the context
    Then the conversation size is 1
    And the view shows:
      """
      Tester: first question
      Sage: First.
      Sage: compacting context...
      Sage: context compacted.
      """
