Feature: Food tools

  The search_food tool returns the stats of food items the model asks about as
  CSV (name, health, stamina, eitr, duration in minutes, healing per tick).

  Background:
    Given the item database contains food:
      | Name        | Health | Stamina | Eitr | BurnTime | Regen |
      | Cooked meat | 40     | 30      | 0    | 1200     | 1     |
      | Magecap     | 0      | 0       | 25   | 1800     | 0     |
      | Raspberry   | 7      | 0       | 0    | 600      | 1     |

  Scenario: Search returns a food's stats as CSV
    When I call the "search_food" tool with query "cooked"
    Then the tool succeeds
    And the tool result contains:
      """
      Name,Health,Stamina,Eitr,Duration (minutes),Healing (hp/tick)
      Cooked meat,40,30,0,20,1
      """
    And the tool result does not contain food "Magecap"

  Scenario: Eitr food reports its eitr
    When I call the "search_food" tool with query "magecap"
    Then the tool succeeds
    And the tool result contains:
      """
      Name,Health,Stamina,Eitr,Duration (minutes),Healing (hp/tick)
      Magecap,0,0,25,30,0
      """

  Scenario: The food tool requires its query argument
    When I call the "search_food" tool without arguments
    Then the tool fails with "Missing required argument 'query'."