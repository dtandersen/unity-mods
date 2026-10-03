Feature: Sage HTTP API

  Pi calls Sage over HTTP. Each call is one GET plus the full JSON answer.

  Background:
    Given my position is "1", "2", "3" on "Europa"

  Scenario: Player status
    When I GET /sage/status
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
    When I GET /sage/mips?instruction=add
    Then the response is:
      """
      [{"Name":"add","Example":"add r? device var","Description":"Does add."}]
      """

  Scenario: IC editor read
    Given the IC editor holds "alias a d0" on "Lab Computer"
    When I GET /sage/ic?computer=lab
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

  Scenario: Stationpedia read
    Given the stationpedia contains:
      | Key       | Title      | Description   | Text                      |
      | ItemCable | Cable Coil | Spooled wire. | Cable Coil carries power. |
    When I GET /sage/stationpedia/read?key=ItemCable
    Then the response is:
      """
      {"Key":"ItemCable","Title":"Cable Coil","Description":"Spooled wire.","Text":"Cable Coil carries power."}
      """

  Scenario: Device describe
    Given the device "Air Vent" exists
    When I GET /sage/devices/describe?device=Air Vent
    Then the response is:
      """
      {"Name":"Air Vent","Type":"StructureAirVent","Thing":101,"NetworkId":7,"IsOn":true,"Logic":[{"Name":"Pressure","Value":101.3,"Writable":false},{"Name":"On","Value":1.0,"Writable":true}],"Slots":[{"Index":0,"Slot":"Filter","Occupant":"empty"}],"Devices":null,"Chip":null}
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

  Scenario: Room read
    Given a sealed room 5 at 20 C and 101 kPa in 100 L holding:
      | Gas      | Moles |
      | Oxygen   | 10    |
      | Nitrogen | 40    |
    And I stand in room 5
    When I GET /sage/room?id=5
    Then the response is:
      """
      {"RoomId":5,"RoomType":"sealed","TemperatureC":20.0,"PressureKpa":101.0,"VolumeLitres":100.0,"Gases":[{"Gas":"Oxygen","Moles":10.0},{"Gas":"Nitrogen","Moles":40.0}]}
      """

  Scenario: IC editor write
    When I POST /sage/ic with:
      """
      {"computer":"lab","code":"alias a d0"}
      """
    Then the response is:
      """
      {"Written":true,"Device":"Lab Computer","Lines":1,"Error":null}
      """

  Scenario: IC export
    When I POST /sage/ic/export with:
      """
      {"computer":"lab","device":"housing"}
      """
    Then the response is:
      """
      {"Written":true,"Device":"Climate Control","Lines":37,"Error":null}
      """
