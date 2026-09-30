Feature: Ask an AI model for recipes

  The assistant is the entry point that talks to an AI model. These tests script
  the model's responses and assert on the conversation the assistant sends back.

  The model never reads the game data: it asks for tools (one at a time), and the
  assistant runs them and feeds the results back.

  Background:
    Given the recipe database contains a "Cable Coil" recipe at "Electronics Printer" with ingredients:
      | Ingredient | Amount |
      | Copper     | 5      |
    And the stationpedia contains:
      | Key       | Title      | Description   | Text                       |
      | ItemCable | Cable Coil | Spooled wire. | Cable Coil carries power.  |

  Scenario: The assistant asks the model, runs the tool, and feeds the recipe back
    Given the AI model will request the tool "search_recipes" with query "coil"
    And the AI model will answer by repeating the last tool result

    When I ask the assistant "How do I make cable coil?"

    Then the request sent back to the AI model should contain these messages:
      | Role      | Content                                                        | Tool call      |
      | user      | How do I make cable coil?                                      |                |
      | assistant |                                                                | search_recipes |
      | tool      | Name,Station,Ingredients\nCable Coil,Electronics Printer,Copper x5 |                |

    And the assistant replies "Name,Station,Ingredients\nCable Coil,Electronics Printer,Copper x5"

  Scenario: The assistant can use two different tools in one conversation
    Given the AI model will request the tool "search_recipes" with query "coil"
    And the AI model will request the tool "search_stationpedia" with query "cable"
    And the AI model will answer "Cable Coil needs Copper x5 and carries power."

    When I ask the assistant "What is cable coil and how do I make it?"

    Then the request sent back to the AI model should contain these messages:
      | Role      | Content                                                        | Tool call          |
      | user      | What is cable coil and how do I make it?                       |                    |
      | assistant |                                                                | search_recipes     |
      | tool      | Name,Station,Ingredients\nCable Coil,Electronics Printer,Copper x5 |                |
      | assistant |                                                                | search_stationpedia |
      | tool      | Title,Key,Description\nCable Coil,ItemCable,Spooled wire.      |                    |

    And the assistant replies "Cable Coil needs Copper x5 and carries power."
