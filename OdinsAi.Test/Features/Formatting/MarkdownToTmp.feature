Feature: Markdown to TextMeshPro

  The assistant replies in Markdown, but Valheim renders TextMeshPro rich text.
  This converter turns a small subset of Markdown into TMP tags before display.
  Tables are rendered as monospaced, padded columns.

  Scenario Outline: Inline styles are converted
    When I convert the markdown "<markdown>"
    Then the text is "<tmp>"

    Examples:
      | markdown           | tmp                              |
      | hello **world**    | hello <b>world</b>               |
      | hello __world__    | hello <b>world</b>               |
      | hello *world*      | hello <i>world</i>               |
      | use `x` now        | use <color=#E0C46C>x</color> now |
      | ~~gone~~           | <s>gone</s>                      |
      | see [docs](http://a.b) | see docs                     |

  Scenario: Multiplication is not italicised
    When I convert the markdown "2 * 3 * 4"
    Then the text is "2 * 3 * 4"

  Scenario: Snake_case is not italicised
    When I convert the markdown "the m_food field"
    Then the text is "the m_food field"

  Scenario: Headings and bullets are converted
    When I convert the markdown:
      """
      # Title
      - one
      - two
      """
    Then the text is:
      """
      <b>Title</b>
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

  Scenario: Tables become monospaced padded columns
    When I convert the markdown:
      """
      | Name  | Qty |
      |-------|-----|
      | Arrow | 20  |
      | Torch | 5   |
      """
    Then the text is:
      """
      <mspace=0.7em>Name   Qty
      -----  ---
      Arrow  20
      Torch  5</mspace>
      """

  Scenario: Column alignment follows the separator row
    When I convert the markdown:
      """
      | Item | Qty |
      |------|----:|
      | Arrow | 20 |
      | Torch | 5  |
      """
    Then the text is:
      """
      <mspace=0.7em>Item   Qty
      -----  ---
      Arrow   20
      Torch    5</mspace>
      """

  Scenario: Text after a table is not monospaced
    When I convert the markdown:
      """
      | A | B |
      |---|---|
      | 1 | 2 |
      after
      """
    Then the text is:
      """
      <mspace=0.7em>A  B
      -  -
      1  2</mspace>
      after
      """

  Scenario: A lone pipe is not a table
    When I convert the markdown "a | b"
    Then the text is "a | b"