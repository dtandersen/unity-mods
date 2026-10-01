Feature: Mention recall

  Mentions in chat belong to one session per player. The second ask sees the
  first question and answer, so Sage does not re-search what it already knows.

  Scenario: Second mention sees the first turn
    Given the AI model will answer "Heater 1208 is on network 1194."
    And the AI model will answer "Sensor 1482 is wired in."
    When the player mentions "!sage heater ic does not work"
    And the player mentions "!sage sensor is wired in"
    Then the AI model's last request contains a "user" message "heater ic does not work"
    And the AI model's last request contains a "assistant" message "Heater 1208 is on network 1194."
    And the AI model's last request contains a "user" message "sensor is wired in"
