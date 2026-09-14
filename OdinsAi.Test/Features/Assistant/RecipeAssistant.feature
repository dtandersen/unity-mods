Feature: Ask an AI model for recipes

  The assistant is the entry point that talks to an AI model. These tests script
  the model's responses and assert on the conversation the assistant sends back.

  The model never reads the recipe database: it asks for a tool, and the
  assistant runs it and feeds the result back.

  Background:
    Given the recipe database contains a "Turnip stew" recipe with ingredients:
      | Ingredient | Amount |
      | Turnip     | 3      |
      | Boar meat  | 1      |

  Scenario: The assistant asks the model, runs the tool, and feeds the recipe back
    Given the AI model will request the tool "search_recipes" with query "stew"
    And the AI model will answer by repeating the last tool result

    When I ask the assistant "How do I make stew?"

    Then the request sent back to the AI model should contain these messages:
      | Role      | Content                              | Tool call      |
      | user      | How do I make stew?                  |                |
      | assistant |                                      | search_recipes |
      | tool      | Turnip stew: Turnip x3, Boar meat x1 |                |

    And the assistant replies "Turnip stew: Turnip x3, Boar meat x1"