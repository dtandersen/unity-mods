Feature: Markdown to plain text

  The assistant replies in Markdown, but the Stationeers console is plain text.
  This converter strips formatting markers before display.

  Scenario Outline: Inline styles are stripped
    When I convert the markdown "<markdown>"
    Then the text is "<plain>"

    Examples:
      | markdown               | plain          |
      | hello **world**        | hello world    |
      | hello __world__        | hello world    |
      | hello *world*          | hello world    |
      | use `x` now            | use x now      |
      | ~~gone~~               | gone           |
      | see [docs](http://a.b) | see docs      |

  Scenario: Multiplication is not italicised
    When I convert the markdown "2 * 3 * 4"
    Then the text is "2 * 3 * 4"

  Scenario: Headings and bullets are converted
    When I convert the markdown:
      """
      # Title
      - one
      - two
      """
    Then the text is:
      """
      Title
      • one
      • two
      """

  Scenario: Code fences are dropped
    When I convert the markdown:
      """
      before
      ```csharp
      var x = 1;
      ```
      after
      """
    Then the text is:
      """
      before
      var x = 1;
      after
      """
