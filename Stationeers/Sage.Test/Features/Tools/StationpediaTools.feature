Feature: Stationpedia tools

  The search_stationpedia and read_stationpedia tools expose the in-game
  encyclopedia so the assistant can answer questions about items, gases, and
  mechanics.

  Background:
    Given the stationpedia contains:
      | Key        | Title   | Description      | Text                                  |
      | ItemSteel  | Steel   | A sturdy alloy.  | Smelt iron and coal in a furnace.     |
      | GasOxygen  | Oxygen  | Breathe it.      | Oxygen supports combustion.           |

  Scenario: Search the stationpedia by title
    When I call the "search_stationpedia" tool with query "steel"
    Then the tool succeeds
    And the stationpedia result titles are "Steel"

  Scenario: Search the stationpedia by key
    When I call the "search_stationpedia" tool with query "GasOxygen"
    Then the tool succeeds
    And the stationpedia result titles are "Oxygen"

  Scenario: The stationpedia search requires its query argument
    When I call the "search_stationpedia" tool without arguments
    Then the tool fails with "Missing required argument 'query'."

  Scenario: Read a stationpedia page
    When I call the "read_stationpedia" tool with key "ItemSteel"
    Then the tool succeeds
    And the stationpedia page "ItemSteel" reads:
      """
      Steel
      Smelt iron and coal in a furnace.
      """

  Scenario: Reading an unknown key fails
    When I call the "read_stationpedia" tool with key "Nope"
    Then the tool fails with "No Stationpedia page found for key 'Nope'."

  Scenario: Read requires its key argument
    When I call the "read_stationpedia" tool without arguments
    Then the tool fails with "Missing required argument 'key'."

  Scenario: Read lists what uses the subject
    Given the stationpedia contains:
      | Key           | Title | Description   | Text              |
      | ThingItemCoal | Coal  | Burns hot.    | Dug from asteroids. |
    And the stationpedia page "ThingItemCoal" is used in:
      | Title                | Detail        | Key                          |
      | Furnace                |               | ThingStructureFurnace        |
      | Generator (Solid Fuel) | 100 kW Energy | ThingStructureSolidGenerator |
    When I call the "read_stationpedia" tool with key "ThingItemCoal"
    Then the tool succeeds
    And the tool result is:
      """
      Coal
      Dug from asteroids.
      Used in:
      Furnace
      Generator (Solid Fuel) - 100 kW Energy
      """

  Scenario: Read lists how to manufacture the subject
    Given the stationpedia contains:
      | Key        | Title | Description   | Text          |
      | ItemSteel2 | Steel | Strong alloy. | Smelt to shape. |
    And the stationpedia page "ItemSteel2" is made in:
      | Station          | Tier     | Detail                                    | Key                   |
      | Advanced Furnace | Tier One | Temperature: 900 K to 100 kK; Iron x0.75 | ThingAdvancedFurnace |
      | Furnace          |          | Iron x0.5                               | ThingStructureFurnace |
    When I call the "read_stationpedia" tool with key "ItemSteel2"
    Then the tool succeeds
    And the tool result is:
      """
      Steel
      Smelt to shape.
      Made in:
      Advanced Furnace (Tier One) - Temperature: 900 K to 100 kK; Iron x0.75
      Furnace - Iron x0.5
      """

  Scenario: Read lists how the subject burns
    Given the stationpedia contains:
      | Key          | Title     | Description | Text  |
      | GasVolatiles | Volatiles | Burns.      | Fuel. |
    And the stationpedia page "GasVolatiles" burns:
      | Fuel | Oxidiser | FuelAmount | OxidiserAmount | Outputs      | AutoIgnitionC | OutputTemperatureC |
      | CH4  | O2       | 2          | 1              | X x3; CO2 x6 | 300           | 2150               |
    When I call the "read_stationpedia" tool with key "GasVolatiles"
    Then the tool succeeds
    And the tool result is:
      """
      Volatiles
      Fuel.
      Combustion:
      2 x CH4 + 1 x O2 -> 3 x X + 6 x CO2; Auto Ignition 300 °C; 2150 °C
      """

  Scenario: Read lists solid fuel numbers for coal
    Given the stationpedia contains:
      | Key           | Title | Description | Text                | FuelEnergyPerSecondW | TickSeconds |
      | ThingItemCoal | Coal  | Burns hot.  | Dug from asteroids. | 100000               | 0.5         |
    And the stationpedia page "ThingItemCoal" is used in:
      | Title                | Detail        | Key                          | EnergyPerSecondW | GeneratorPowerW | BurnTicks | TotalJoules | BurnSeconds | TickSeconds |
      | Generator (Solid Fuel) | 100 kW Energy | ThingStructureSolidGenerator | 100000           | 20000           | 10        | 200000      | 5           | 0.5         |
    When I call the "read_stationpedia" tool with key "ThingItemCoal"
    Then the tool succeeds
    And the tool result is:
      """
      Coal
      Dug from asteroids.
      Specs:
      Fuel energy: 100000 W
      Tick: 0.5 s
      Used in:
      Generator (Solid Fuel) - 100 kW Energy (100000 W, 10 ticks, 5 s, 200000 J per unit, 0.5 s per tick)
      """

  Scenario: Read lists power specs for generator and battery
    Given the stationpedia contains:
      | Key                          | Title                  | Description   | Text        | PowerGenerationW | PowerStorageJ | TickSeconds |
      | ThingStructureSolidGenerator | Generator (Solid Fuel) | Burns fuel.   | Burns coal. | 20000            |               | 0.5         |
      | ThingStructureBattery        | Battery                | Stores power. | Holds charge. |                  | 3600000       |             |
    When I call the "read_stationpedia" tool with key "ThingStructureSolidGenerator"
    Then the tool succeeds
    And the tool result is:
      """
      Generator (Solid Fuel)
      Burns coal.
      Specs:
      Power generation: 20000 W
      Tick: 0.5 s
      """
    When I call the "read_stationpedia" tool with key "ThingStructureBattery"
    Then the tool succeeds
    And the tool result is:
      """
      Battery
      Holds charge.
      Specs:
      Power storage: 3600000 J
      """

  Scenario: Read lists how to place the subject
    Given the stationpedia contains:
      | Key                 | Title   | Description | Text   |
      | ThingStructureFurnace | Furnace | Smelts ore. | Hot. |
    And the stationpedia page "ThingStructureFurnace" builds in:
      | Step                          |
      | Kit (Furnace) x1              |
      | Wrench - Iron Sheets x2       |
    When I call the "read_stationpedia" tool with key "ThingStructureFurnace"
    Then the tool succeeds
    And the tool result is:
      """
      Furnace
      Hot.
      Build:
      Kit (Furnace) x1
      Wrench - Iron Sheets x2
      """

  Scenario: Read lists logic and connections for a logic writer
    Given the stationpedia contains:
      | Key                       | Title        | Description   | Text           |
      | ThingStructureLogicWriter | Logic Writer | Writes logic. | Links devices. |
    And the stationpedia page "ThingStructureLogicWriter" has logic:
      | Name       | Access     |
      | Power      | Read       |
      | On         | Read Write |
      | ForceWrite | Write      |
    And the stationpedia page "ThingStructureLogicWriter" has connections:
      | Name        | Index |
      | Data Input  | 0     |
      | Data Output | 1     |
      | Connection  | 2     |
    When I call the "read_stationpedia" tool with key "ThingStructureLogicWriter"
    Then the tool succeeds
    And the tool result is:
      """
      Logic Writer
      Links devices.
      Logic:
      Power - Read
      On - Read Write
      ForceWrite - Write
      Connections:
      Data Input 0
      Data Output 1
      Connection 2
      """
