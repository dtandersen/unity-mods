Feature: Sage presenter

  The presenter drives the Sage window's passive view and runs each turn:
  console and chat questions echo into the window history, a busy window
  takes no second turn until the answer finishes, the status bar shows Idle
  when nothing runs and the live thinking time while a turn runs, and the
  toggle key shows and hides the window.
  The window has no input: it shows history and status only.

  Scenario: The status starts idle
    Then the status is "Idle"

  Scenario: The update loop leaves the idle status alone
    When the update loop runs
    Then the status is "Idle"

  Scenario: The update loop refreshes the thinking status in real time
    Given the AI model will wait
    And the AI model will answer "done"
    When the console asks "hold on"
    And 6.2 seconds have passed
    And the update loop runs
    Then the status is "Sage: Thinking (6.2s)..."
    When the answer finishes
    And the turn completes
    Then the status is "Idle"

  Scenario: A console question runs a turn
    Given the AI model will answer "42"
    When the console asks "what is the answer"
    And the turn completes
    Then the asked question is "what is the answer"
    And the window is not busy
    And the status is "Idle"
    And the view shows:
      """
      Tester: what is the answer
      Sage: 42
      """

  Scenario: An empty console question asks nothing
    When the console asks "   "
    Then nothing is asked
    And the window is not busy
    And the status is "Idle"

  Scenario: A busy window accepts no question
    Given the assistant is still answering "first question"
    When the console asks "second question"
    Then the asked question is "first question"

  Scenario: The status shows thinking while a turn runs
    Given the AI model will wait
    And the AI model will answer "done"
    When the console asks "hold on"
    Then the window is busy
    And the status is "Sage: Thinking (0.0s)..."
    When the answer finishes
    And the turn completes
    Then the status is "Idle"

  Scenario: The window leaves busy when the turn finishes
    Given the assistant is still answering "hello"
    When the answer finishes
    Then the window is not busy
    And the status is "Idle"

  Scenario: The answer shows as plain text
    Given the AI model will answer "Use **copper** and `cable`"
    When the console asks "what do I need?"
    And the turn completes
    Then the view shows:
      """
      Tester: what do I need?
      Sage: Use copper and cable
      """

  Scenario: A console question is ignored while busy
    Given the assistant is still answering "first question"
    When the console asks "second question"
    Then the asked question is "first question"

  Scenario: A missing player name falls back to You
    Given my player name is ""
    And the AI model will answer "hi"
    When the console asks "hello"
    And the turn completes
    Then the view shows:
      """
      You: hello
      Sage: hi
      """

  Scenario: The assistant name is used
    Given the assistant is named "Ada"
    And the AI model will answer "hi"
    When the console asks "hello"
    And the turn completes
    Then the view shows:
      """
      Tester: hello
      Ada: hi
      """

  Scenario: Reasoning is shown when thinking is on
    Given the AI model reasons "weigh the options" and answers "Go north"
    And thinking display is on
    When the console asks "which way"
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
    When the console asks "how do I make cable?"
    And the turn completes
    Then the view shows:
      """
      Tester: how do I make cable?
      Sage: done
      """

  Scenario: Hidden activity shows in the status bar, not the history
    Given the AI model will request the tool "search_items" with query "cable"
    And the AI model will answer "done"
    And the AI model will pause after 1 calls
    And 6 seconds have passed
    When the console asks "how do I make cable?"
    And the turn pauses
    Then the view shows:
      """
      Tester: how do I make cable?
      """
    And the status is "Sage: Thinking (6.0s)..."
    When the answer finishes
    And the turn completes
    Then the view shows:
      """
      Tester: how do I make cable?
      Sage: done
      """
    And the status is "Idle"

  Scenario: The thinking status never lands in the history
    Given the AI model will request the tool "search_items" with query "cable"
    And the AI model will answer "done"
    And 6 seconds have passed
    When the console asks "how do I make cable?"
    And the turn completes
    Then the view shows:
      """
      Tester: how do I make cable?
      Sage: done
      """
    And the status is "Idle"

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
    When the console asks "first question"
    And the turn completes
    Given the AI model will answer "Summary."
    When the user compacts the context
    Then the conversation holds 1 message
    And the view shows:
      """
      Tester: first question
      Sage: First.
      Sage: compacting context...
      Sage: context compacted.
      """
