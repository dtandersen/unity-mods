Feature: Ask an AI model for recipes

  The assistant is the entry point that talks to an AI model. These tests script
  the model's responses and assert on the conversation the assistant sends back.

  The model never reads the game data: it asks for tools (one at a time), and the
  assistant runs them and feeds the results back.

  Background:
    Given the recipe database contains a "Turnip stew" recipe with ingredients:
      | Ingredient | Amount |
      | Turnip     | 3      |
      | Boar meat  | 1      |
    And the item database contains food:
      | Name        | Health | Stamina | Eitr | BurnTime | Regen |
      | Cooked meat | 40     | 30      | 0    | 1200     | 1     |

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

  Scenario: The assistant can use two different tools in one conversation
    Given the AI model will request the tool "search_recipes" with query "stew"
    And the AI model will request the tool "search_food" with query "cooked"
    And the AI model will answer "Turnip stew needs Turnip x3 and Boar meat x1; Cooked meat gives 40 health."

    When I ask the assistant "What can I cook from a turnip, and what food is best?"

    Then the request sent back to the AI model should contain these messages:
      | Role      | Content                                                        | Tool call      |
      | user      | What can I cook from a turnip, and what food is best?          |                |
      | assistant |                                                                | search_recipes |
      | tool      | Turnip stew: Turnip x3, Boar meat x1                           |                |
      | assistant |                                                                | search_food    |
      | tool      | Name,Health,Stamina,Eitr,BurnTime,Regen\nCooked meat,40,30,0,1200,1 |                |

    And the assistant replies "Turnip stew needs Turnip x3 and Boar meat x1; Cooked meat gives 40 health."