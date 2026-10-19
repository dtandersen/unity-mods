Feature: Sage HTTP API

  Pi calls Sage over HTTP. Each call is one GET plus the full JSON answer.

  Background:
    Given my position is "1", "2", "3" on "Europa"

  Scenario: Player status
    When I GET /sage/players
    Then the response is:
      """
      {
        "X": 1.0,
        "Y": 2.0,
        "Z": 3.0,
        "World": "Europa",
        "Details": null,
        "RoomId": null,
        "FacingYaw": null
      }
      """

  Scenario: Inventory search
    Given my inventory contains:
      | Item       | Slot     | Quantity |
      | Cable Coil | Toolbelt | x5       |
      | Iron Ingot | Backpack | x10      |
    When I GET /sage/inventory?q=coil
    Then the response is:
      """
      [
        {
          "PrefabName": "Cable Coil",
          "Name": "Cable Coil",
          "Quantity": "x5",
          "Slot": "Toolbelt"
        }
      ]
      """

  Scenario: Inventory search reports the hand
    Given my inventory contains:
      | Item   | Slot       |
      | Welder | Right hand |
    When I GET /sage/inventory?q=welder
    Then the response is:
      """
      [
        {
          "PrefabName": "Welder",
          "Name": "Welder",
          "Quantity": null,
          "Slot": "Right hand",
          "Hand": "Right"
        }
      ]
      """

  Scenario: Device search
    Given live devices exist:
      | Name       | PrefabName         | Id    | Network | Logic | Room | Position |
      | Air Vent   | StructureAirVent   | 101   | 7       | true  | 5    | 1,2,3    |
      | Gas Sensor | StructureGasSensor | 102   | 0       | true  | 5    | 4,5,6    |
    When I GET /sage/devices?q=vent
    Then the response is:
      """
      [
        {
          "Name": "Air Vent",
          "PrefabName": "StructureAirVent",
          "Id": 101,
          "NetworkId": 7,
          "Logic": true,
          "RoomId": 5,
          "position": {
            "x": 1.0,
            "y": 2.0,
            "z": 3.0
          }
        }
      ]
      """

  Scenario: Item catalogue search
    Given the item database contains:
      | Name       | ItemType |
      | Cable Coil | Item     |
    When I GET /sage/items?q=coil
    Then the response is:
      """
      [
        {
          "PrefabName": "Cable Coil",
          "Name": "Cable Coil",
          "ItemType": "Item"
        }
      ]
      """

  Scenario: Recipe search
    Given the recipe database contains a "Cable Coil" recipe at "Electronics Printer" with ingredients:
      | Ingredient | Amount |
      | Copper     | 5      |
    When I GET /sage/recipes?q=coil
    Then the response is:
      """
      [
        {
          "Id": "Cable Coil",
          "ResultItemId": "Cable Coil",
          "Name": "Cable Coil",
          "Amount": 1,
          "Station": "Electronics Printer",
          "Ingredients": [
            {
              "ItemId": "Copper",
              "Amount": 5.0
            }
          ]
        }
      ]
      """

  Scenario: Stationpedia search
    Given the stationpedia contains:
      | Key       | Title      | Description  | Text                      |
      | ItemCable | Cable Coil | Spooled wire. | Cable Coil carries power. |
    When I GET /sage/stationpedia?q=cable
    Then the response is:
      """
      [
        {
          "Key": "ItemCable",
          "Title": "Cable Coil",
          "Description": "Spooled wire.",
          "Text": "Cable Coil carries power."
        }
      ]
      """

  Scenario: MIPS reference
    Given the MIPS reference contains "add" and "sub"
    When I GET /sage/instructions?instruction=add
    Then the response is:
      """
      [
        {
          "Name": "add",
          "Example": "add r? device var",
          "Description": "Does add."
        }
      ]
      """

  Scenario: Circuit housing read
    Given the housing holds "alias a d0" on "Climate Control"
    When I GET /sage/circuit-housings/2200
    Then the response is:
      """
      {
        "SourceCode": "alias a d0",
        "Hash": "8044fec0"
      }
      """

  Scenario: Computer read
    Given the IC editor holds "alias a d0" on "Lab Computer"
    And the computer points at "Climate Control"
    When I GET /sage/computer/1189
    Then the response is:
      """
      {
        "Name": "Lab Computer",
        "Device": 2200,
        "SourceCode": "alias a d0",
        "Hash": "8044fec0"
      }
      """

  Scenario: Unknown computer read
    Given no computer is found
    When I GET /sage/computer/999
    Then the response status is 404
    And the response reports "Computer 999 not found."

  Scenario: Faulty computer read answers 500
    Given the computer read faults
    When I GET /sage/computer/1189
    Then the response status is 500
    And the response reports "Command failed."

  Scenario: Computer read by label is refused
    When I GET /sage/computer/lab
    Then the response status is 400
    And the response reports "computer must be a thing id."

  Scenario: World read
    Given a world
    When I GET /sage/world
    Then the response is:
      """
      {
        "WorldName": "Europa",
        "WorldId": "Europa1",
        "Gravity": -9.8,
        "DaysPast": 12,
        "HasLava": false,
        "IsCreative": false,
        "DayLength": "20m",
        "YearLength": "60d",
        "DaysPerYear": 72.0,
        "Eccentricity": 0.01,
        "SemiMajorAxisAu": 5.2,
        "Inclination": 3.1,
        "TidallyLocked": false,
        "SolarIrradiance": 500.0,
        "EarthSolarRatio": 0.37,
        "TimeOfDay": 0.35,
        "IsEclipse": false,
        "OutsideTemperatureC": -20.0,
        "OutsidePressureKpa": 0.0,
        "OutsideAir": [
          {
            "MoleType": "Nitrogen",
            "Percent": 90.0
          },
          {
            "MoleType": "Oxygen",
            "Percent": 10.0
          }
        ]
      }
      """

  Scenario: Structure catalogue search
    Given the structure database contains:
      | Name       | Description |
      | Air Vent   | Moves air.  |
    When I GET /sage/structures?q=vent
    Then the response is:
      """
      [
        {
          "PrefabName": "Air Vent",
          "Name": "Air Vent",
          "Description": "Moves air.",
          "BuildStates": []
        }
      ]
      """

  Scenario: Structure names ship without rich text
    Given the structure database contains:
      | Name                          | Description |
      | <color=green>Autolathe</color> | Prints.     |
    When I GET /sage/structures?q=autolathe
    Then the response is:
      """
      [
        {
          "PrefabName": "Autolathe",
          "Name": "Autolathe",
          "Description": "Prints.",
          "BuildStates": []
        }
      ]
      """

  Scenario: Stationpedia read
    Given the stationpedia contains:
      | Key       | Title      | Description   | Text                      |
      | ItemCable | Cable Coil | Spooled wire. | Cable Coil carries power. |
    When I GET /sage/stationpedia/ItemCable
    Then the response is:
      """
      {
        "Key": "ItemCable",
        "Title": "Cable Coil",
        "Description": "Spooled wire.",
        "Text": "Cable Coil carries power."
      }
      """

  Scenario: Stationpedia text ships without rich text
    Given the stationpedia contains:
      | Key     | Title                 | Description                 | Text   |
      | ItemFan | <color=red>Fan</color> | Moves <link=Air>air</link>. | Plain. |
    When I GET /sage/stationpedia/ItemFan
    Then the response is:
      """
      {
        "Key": "ItemFan",
        "Title": "Fan",
        "Description": "Moves air.",
        "Text": "Plain."
      }
      """

  Scenario: Device describe
    Given the device "Air Vent" exists
    When I GET /sage/devices/Air Vent
    Then the response is:
      """
      {
        "Name": "Air Vent",
        "PrefabName": "StructureAirVent",
        "Id": 101,
        "NetworkId": 7,
        "IsOn": true,
        "Logic": [
          {
            "Name": "Pressure",
            "Value": 101.3,
            "Writable": false
          },
          {
            "Name": "On",
            "Value": 1.0,
            "Writable": true
          }
        ],
        "Slots": [
          {
            "Index": 0,
            "SlotType": "Filter",
            "Occupant": "empty"
          }
        ],
        "Devices": null,
        "Chip": null,
        "Storage": null,
        "Build": null
      }
      """

  Scenario: Device describe includes fabricator storage
    Given the device "Autolathe" holds:
      | Reagent | Quantity |
      | Iron    | 60       |
      | Copper  | 12.5     |
    When I GET /sage/devices/1456
    Then the response is:
      """
      {
        "Name": "Autolathe",
        "PrefabName": "StructureAutolathe",
        "Id": 1456,
        "NetworkId": 0,
        "IsOn": false,
        "Logic": [],
        "Slots": [
          {
            "Index": 0,
            "SlotType": "Import",
            "Occupant": "empty"
          },
          {
            "Index": 1,
            "SlotType": "Export",
            "Occupant": "empty"
          }
        ],
        "Devices": null,
        "Chip": null,
        "Storage": [
          {
            "Reagent": "Iron",
            "Quantity": 60.0
          },
          {
            "Reagent": "Copper",
            "Quantity": 12.5
          }
        ],
        "Build": null
      }
      """

  Scenario: Thing search
    Given these world things exist:
      | Name       | PrefabName | Id  | Quantity | X | Y | Z | Room |
      | Iron Ingot | Ingot | 501 | x5       | 1 | 2 | 4 | 0    |
    When I GET /sage/things?q=ingot
    Then the response is:
      """
      {
        "things": [
          {
            "Name": "Iron Ingot",
            "PrefabName": "Ingot",
            "Id": 501,
            "Quantity": "x5",
            "X": 1.0,
            "Y": 2.0,
            "Z": 4.0,
            "RoomId": 0
          }
        ],
        "origin": {
          "X": 1.0,
          "Y": 2.0,
          "Z": 3.0
        },
        "facing": null
      }
      """

  Scenario: Thing slots list container slots
    Given the "Mining Belt" holds:
      | SlotType | Item       | Quantity |
      | Slot 1   | Iron Ore   | x12      |
      | Slot 2   | Copper Ore | x4       |
    When I GET /sage/things/301/slots
    Then the response is:
      """
      [
        {
          "SlotType": "Slot 1",
          "Name": "Iron Ore",
          "Quantity": "x12"
        },
        {
          "SlotType": "Slot 2",
          "Name": "Copper Ore",
          "Quantity": "x4"
        }
      ]
      """

  Scenario: Thing quantity ships without rich text
    Given these world things exist:
      | Name           | PrefabName | Id  | Quantity              | X   | Y   | Z   | Room |
      | Methane Filter | Filter     | 502 | 100<size=75%>%</size> | 1   | 2   | 4   | 0    |
    When I GET /sage/things?q=methane
    Then the response is:
      """
      {
        "things": [
          {
            "Name": "Methane Filter",
            "PrefabName": "Filter",
            "Id": 502,
            "Quantity": "100%",
            "X": 1.0,
            "Y": 2.0,
            "Z": 4.0,
            "RoomId": 0
          }
        ],
        "origin": {
          "X": 1.0,
          "Y": 2.0,
          "Z": 3.0
        },
        "facing": null
      }
      """

  Scenario: Room read
    Given a sealed room 5 at 20 C and 101 kPa in 100 L holding:
      | MoleType | Moles |
      | Oxygen   | 10    |
      | Nitrogen | 40    |
    And I stand in room 5
    When I GET /sage/rooms/5
    Then the response is:
      """
      {
        "RoomId": 5,
        "RoomType": "sealed",
        "TemperatureC": 20.0,
        "PressureKpa": 101.0,
        "VolumeLitres": 100.0,
        "Gases": [
          {
            "MoleType": "Oxygen",
            "Moles": 10.0
          },
          {
            "MoleType": "Nitrogen",
            "Moles": 40.0
          }
        ]
      }
      """

  Scenario: IC editor write
    Given the chip under the dropdown holds "yield"
    When I PUT /sage/computer/1189 with:
      """
      {
        "sourceCode": "alias a d0",
        "hash": "6c96f2ae"
      }
      """
    Then the response is:
      """
      {
        "Device": "Lab Computer",
        "Lines": 1
      }
      """

  Scenario: IC editor write without a chip hash
    When I PUT /sage/computer/1189 with:
      """
      {
        "sourceCode": "alias a d0"
      }
      """
    Then the response status is 400
    And the response reports "A chip hash is required to write: name the hash of the chip under the dropdown."

  Scenario: IC export
    Given the export succeeds
    When I PUT /sage/computer/1189/export with:
      """
      {
        "device": "2200"
      }
      """
    Then the response is:
      """
      {
        "Device": "Climate Control",
        "Lines": 37
      }
      """

  Scenario: IC import
    Given the chip of housing "Climate Control" holds:
      """
      move r0 1
      wait
      yield
      """
    When I PUT /sage/computer/1189/import with:
      """
      {}
      """
    Then the response is:
      """
      {
        "Device": "Lab Computer",
        "Lines": 3
      }
      """

  Scenario: IC select housing
    When I PUT /sage/computer/1189/select with:
      """
      {
        "device": "2200"
      }
      """
    Then the response is:
      """
      {
        "Device": "Climate Control",
        "Lines": 0
      }
      """

  Scenario: IC burn names a housing by label
    When I PUT /sage/computer/1189/export with:
      """
      {
        "device": "Climate Control"
      }
      """
    Then the response status is 400
    And the response reports "device must be a thing id."

  Scenario: Screen write without text to write
    When I PUT /sage/computer/1189 with:
      """
      {
        "hash": "6c96f2ae"
      }
      """
    Then the response status is 400
    And the response reports "sourceCode is required."

  Scenario: A body that is not json is refused
    When I PUT /sage/computer/1189 with:
      """
      sourceCode = yield
      """
    Then the response status is 400
    And the response reports "body is not valid json."
