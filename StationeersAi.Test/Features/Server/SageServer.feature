Feature: Sage HTTP API

  Pi calls Sage over HTTP. Each call is one GET plus the full JSON answer.

  Background:
    Given my position is "1", "2", "3" on "Europa"

  Scenario: Player status
    When I GET /sage/players
    Then the response is:
      """
      {"X":1.0,"Y":2.0,"Z":3.0,"World":"Europa","Details":null,"RoomId":null,"FacingYaw":null}
      """

  Scenario: Inventory search
    Given my inventory contains:
      | Item       | Slot      |
      | Cable Coil | Toolbelt  |
      | Iron Ingot | Backpack  |
    When I GET /sage/inventory?q=coil
    Then the response is:
      """
      [{"Id":"Cable Coil","Name":"Cable Coil","Quantity":null,"Slot":"Toolbelt"}]
      """

  Scenario: Device search
    Given live devices exist:
      | Name      | Type              | Thing | Network | Logic | Room |
      | Air Vent  | StructureAirVent  | 101   | 7       | true  | 5    |
      | Gas Sensor | StructureGasSensor | 102  | 0       | true  | 5    |
    When I GET /sage/devices?q=vent
    Then the response is:
      """
      [{"Name":"Air Vent","Type":"StructureAirVent","Thing":101,"NetworkId":7,"Logic":true,"RoomId":5}]
      """

  Scenario: Item catalogue search
    Given the item database contains:
      | Name       | Type  |
      | Cable Coil | Item  |
    When I GET /sage/items?q=coil
    Then the response is:
      """
      [{"Id":"Cable Coil","Name":"Cable Coil","Type":"Item"}]
      """

  Scenario: Recipe search
    Given the recipe database contains a "Cable Coil" recipe at "Electronics Printer" with ingredients:
      | Ingredient | Amount |
      | Copper     | 5      |
    When I GET /sage/recipes?q=coil
    Then the response is:
      """
      [{"Id":"Cable Coil","ResultItemId":"Cable Coil","Name":"Cable Coil","Amount":1,"Station":"Electronics Printer","Ingredients":[{"ItemId":"Copper","Amount":5.0}]}]
      """

  Scenario: Stationpedia search
    Given the stationpedia contains:
      | Key       | Title      | Description  | Text                      |
      | ItemCable | Cable Coil | Spooled wire. | Cable Coil carries power. |
    When I GET /sage/stationpedia?q=cable
    Then the response is:
      """
      [{"Key":"ItemCable","Title":"Cable Coil","Description":"Spooled wire.","Text":"Cable Coil carries power."}]
      """

  Scenario: MIPS reference
    Given the MIPS reference contains "add" and "sub"
    When I GET /sage/instructions?instruction=add
    Then the response is:
      """
      [{"Name":"add","Example":"add r? device var","Description":"Does add."}]
      """

  Scenario: IC editor read
    Given the IC editor holds "alias a d0" on "Lab Computer"
    When I GET /sage/ic/lab
    Then the response is:
      """
      {"Found":true,"Device":"Lab Computer","Code":"alias a d0","Error":null}
      """

  Scenario: World read
    Given a world
    When I GET /sage/world
    Then the response is:
      """
      {"WorldName":"Europa","WorldId":"Europa1","Gravity":-9.8,"DaysPast":12,"HasLava":false,"IsCreative":false,"DayLength":"20m","YearLength":"60d","DaysPerYear":72.0,"Eccentricity":0.01,"SemiMajorAxisAu":5.2,"Inclination":3.1,"TidallyLocked":false,"SolarIrradiance":500.0,"EarthSolarRatio":0.37,"TimeOfDay":0.35,"IsEclipse":false,"OutsideTemperatureC":-20.0,"OutsidePressureKpa":0.0,"OutsideAir":[{"Gas":"Nitrogen","Percent":90.0},{"Gas":"Oxygen","Percent":10.0}]}
      """

  Scenario: Structure catalogue search
    Given the structure database contains:
      | Name       | Description |
      | Air Vent   | Moves air.  |
    When I GET /sage/structures?q=vent
    Then the response is:
      """
      [{"Id":"Air Vent","Name":"Air Vent","Description":"Moves air."}]
      """

  Scenario: Structure names ship without rich text
    Given the structure database contains:
      | Name                          | Description |
      | <color=green>Autolathe</color> | Prints.     |
    When I GET /sage/structures?q=autolathe
    Then the response is:
      """
      [{"Id":"Autolathe","Name":"Autolathe","Description":"Prints."}]
      """

  Scenario: Stationpedia read
    Given the stationpedia contains:
      | Key       | Title      | Description   | Text                      |
      | ItemCable | Cable Coil | Spooled wire. | Cable Coil carries power. |
    When I GET /sage/stationpedia/ItemCable
    Then the response is:
      """
      {"Key":"ItemCable","Title":"Cable Coil","Description":"Spooled wire.","Text":"Cable Coil carries power."}
      """

  Scenario: Stationpedia text ships without rich text
    Given the stationpedia contains:
      | Key     | Title                 | Description                 | Text   |
      | ItemFan | <color=red>Fan</color> | Moves <link=Air>air</link>. | Plain. |
    When I GET /sage/stationpedia/ItemFan
    Then the response is:
      """
      {"Key":"ItemFan","Title":"Fan","Description":"Moves air.","Text":"Plain."}
      """

  Scenario: Device describe
    Given the device "Air Vent" exists
    When I GET /sage/devices/Air Vent
    Then the response is:
      """
      {"Name":"Air Vent","Type":"StructureAirVent","Thing":101,"NetworkId":7,"IsOn":true,"Logic":[{"Name":"Pressure","Value":101.3,"Writable":false},{"Name":"On","Value":1.0,"Writable":true}],"Slots":[{"Index":0,"Slot":"Filter","Occupant":"empty"}],"Devices":null,"Chip":null,"Storage":null}
      """

  Scenario: Device describe includes fabricator storage
    Given the device "Autolathe" holds:
      | Reagent | Quantity |
      | Iron    | 60       |
      | Copper  | 12.5     |
    When I GET /sage/devices/1456
    Then the response is:
      """
      {"Name":"Autolathe","Type":"StructureAutolathe","Thing":1456,"NetworkId":0,"IsOn":false,"Logic":[],"Slots":[{"Index":0,"Slot":"Import","Occupant":"empty"},{"Index":1,"Slot":"Export","Occupant":"empty"}],"Devices":null,"Chip":null,"Storage":[{"Reagent":"Iron","Quantity":60.0},{"Reagent":"Copper","Quantity":12.5}]}
      """

  Scenario: Thing search
    Given these world things exist:
      | Name       | Type | Thing | Quantity | X | Y | Z | Room |
      | Iron Ingot | Ingot | 501  | x5       | 1 | 2 | 4 | 0    |
    When I GET /sage/things?q=ingot
    Then the response is:
      """
      {"things":[{"Name":"Iron Ingot","Type":"Ingot","Thing":501,"Quantity":"x5","X":1.0,"Y":2.0,"Z":4.0,"RoomId":0}],"origin":{"X":1.0,"Y":2.0,"Z":3.0},"facing":null}
      """

  Scenario: Thing slots list container slots
    Given the "Mining Belt" holds:
      | Slot   | Item       | Quantity |
      | Slot 1 | Iron Ore   | x12      |
      | Slot 2 | Copper Ore | x4       |
    When I GET /sage/things/301/slots
    Then the response is:
      """
      [{"Slot":"Slot 1","Name":"Iron Ore","Quantity":"x12"},{"Slot":"Slot 2","Name":"Copper Ore","Quantity":"x4"}]
      """

  Scenario: Thing quantity ships without rich text
    Given these world things exist:
      | Name           | Type   | Thing | Quantity              | X   | Y   | Z   | Room |
      | Methane Filter | Filter | 502   | 100<size=75%>%</size> | 1   | 2   | 4   | 0    |
    When I GET /sage/things?q=methane
    Then the response is:
      """
      {"things":[{"Name":"Methane Filter","Type":"Filter","Thing":502,"Quantity":"100%","X":1.0,"Y":2.0,"Z":4.0,"RoomId":0}],"origin":{"X":1.0,"Y":2.0,"Z":3.0},"facing":null}
      """

  Scenario: Room read
    Given a sealed room 5 at 20 C and 101 kPa in 100 L holding:
      | Gas      | Moles |
      | Oxygen   | 10    |
      | Nitrogen | 40    |
    And I stand in room 5
    When I GET /sage/rooms/5
    Then the response is:
      """
      {"RoomId":5,"RoomType":"sealed","TemperatureC":20.0,"PressureKpa":101.0,"VolumeLitres":100.0,"Gases":[{"Gas":"Oxygen","Moles":10.0},{"Gas":"Nitrogen","Moles":40.0}]}
      """

  Scenario: IC editor write
    When I POST /sage/ic/lab with:
      """
      {"computer":"lab","code":"alias a d0"}
      """
    Then the response is:
      """
      {"Written":true,"Device":"Lab Computer","Lines":1,"Error":null}
      """

  Scenario: IC export
    When I PUT /sage/ic/lab/export with:
      """
      {"device":"housing"}
      """
    Then the response is:
      """
      {"Written":true,"Device":"Climate Control","Lines":37,"Error":null}
      """
