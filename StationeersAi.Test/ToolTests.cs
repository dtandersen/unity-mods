using Shouldly;
using StationeersAi.Command;
using StationeersAi.Entity;
using StationeersAi.Query;
using StationeersAi.Repository;
using Caev.Tools;
using StationeersAi.Tool;

namespace StationeersAi.Test;

/// <summary>Tool argument handling, failure paths, and model-facing text.</summary>
public class ToolTests
{
    private static ToolHandler Tools()
    {
        return Tools(new InMemoryPlayerContext(), new InMemoryWorldContext(), new InMemoryIcDevices(), new InMemoryMipsReference(), new InMemoryDeviceCatalog());
    }

    private static ToolHandler Tools(InMemoryPlayerContext player, InMemoryWorldContext world, InMemoryIcDevices computers, InMemoryMipsReference mips)
    {
        return Tools(player, world, computers, mips, new InMemoryDeviceCatalog());
    }

    private static ToolHandler Tools(InMemoryPlayerContext player, InMemoryWorldContext world, InMemoryIcDevices computers, InMemoryMipsReference mips, InMemoryDeviceCatalog devices)
    {
        var items = new InMemoryItemRepository(new[]
        {
            new Item("ItemSteel", "Steel", "Ingot"),
            new Item("ItemCopper", "Copper", "Ore"),
        });
        var structures = new InMemoryStructureRepository(new[]
        {
            new Structure("StructureSolarPanel", "Solar Panel", "Generates power."),
        });
        var recipes = new InMemoryRecipeRepository(new[]
        {
            new Recipe("ItemCableCoil", "ItemCableCoil", "Cable Coil", 1, "Electronics Printer",
                new[] { new Ingredient("Copper", 5), new Ingredient("Plastic", 0.5) }),
        });
        var pages = new InMemoryStationpediaRepository(new[]
        {
            new StationpediaEntry("ItemSteel", "Steel", "A sturdy alloy.", "Smelt iron and coal..."),
        });

        return new ToolHandler(new ITool[]
        {
            new SearchItemsTool(new SearchItemsCommand(new ItemIndex(items))),
            new SearchStructuresTool(new SearchStructuresCommand(new StructureIndex(structures))),
            new SearchRecipesTool(new SearchRecipesCommand(new RecipeIndex(recipes))),
            new SearchStationpediaTool(new SearchStationpediaCommand(new StationpediaIndex(pages))),
            new ReadStationpediaTool(new ReadStationpediaCommand(pages)),
            new SearchInventoryTool(new SearchInventoryCommand(new InventoryIndex(player))),
            new DescribePlayerTool(new DescribePlayerCommand(player)),
            new GetWorldTool(new GetWorldCommand(world)),
            new WriteIcTool(new UseComputerCommand(computers)),
            new ListInstructionsTool(new ListInstructionsCommand(mips)),
            new SearchDevicesTool(new SearchDevicesCommand(devices)),
        });
    }

    private static ToolResult Call(ToolHandler tools, string name, params (string key, string value)[] args)
    {
        var values = new System.Collections.Generic.Dictionary<string, string>();
        foreach (var (key, value) in args)
        {
            values[key] = value;
        }

        return tools.Handle(new ToolCall("1", name, new ToolArguments(values)));
    }

    [Test]
    public void Search_items_returns_csv()
    {
        var result = Call(Tools(), "search_items", ("query", "steel"));
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("Name,Type,Id");
        result.Content.ShouldContain("Steel,Ingot,ItemSteel");
    }

    [Test]
    public void Search_items_missing_query_fails()
    {
        var result = Call(Tools(), "search_items");
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("query");
    }

    [Test]
    public void Search_items_no_match_says_so()
    {
        var result = Call(Tools(), "search_items", ("query", "plastic"));
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldBe("No items found for 'plastic'.");
    }

    [Test]
    public void Search_structures_returns_csv()
    {
        var result = Call(Tools(), "search_structures", ("query", "solar"));
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("Solar Panel");
        result.Content.ShouldContain("StructureSolarPanel");
    }

    [Test]
    public void Search_recipes_lists_ingredients()
    {
        var result = Call(Tools(), "search_recipes", ("query", "cable"));
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("Name,Station,Ingredients");
        result.Content.ShouldContain("Cable Coil");
        result.Content.ShouldContain("Electronics Printer");
        result.Content.ShouldContain("Copper x5");
        result.Content.ShouldContain("Plastic x0.5");
    }

    [Test]
    public void Search_recipes_no_match_says_so()
    {
        var result = Call(Tools(), "search_recipes", ("query", "glass"));
        result.Content.ShouldBe("No recipes found for 'glass'.");
    }

    [Test]
    public void Search_stationpedia_returns_keys_for_reading()
    {
        var result = Call(Tools(), "search_stationpedia", ("query", "steel"));
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("Title,Key,Description");
        result.Content.ShouldContain("ItemSteel");
    }

    [Test]
    public void Read_stationpedia_returns_page_text()
    {
        var result = Call(Tools(), "read_stationpedia", ("key", "ItemSteel"));
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("Steel");
        result.Content.ShouldContain("Smelt iron and coal");
    }

    [Test]
    public void Read_stationpedia_unknown_key_fails()
    {
        var result = Call(Tools(), "read_stationpedia", ("key", "Nope"));
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("Nope");
    }

    [Test]
    public void Search_inventory_reports_slot_and_quantity()
    {
        var player = new InMemoryPlayerContext();
        player.Carry(new ItemStack("ItemWelder", "Welder", "85%", "Right hand"));

        var result = Call(Tools(player, new InMemoryWorldContext(), new InMemoryIcDevices(), new InMemoryMipsReference()), "search_inventory", ("query", "weld"));
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("Welder,85%,Right hand");
    }

    [Test]
    public void Describe_player_reports_position_and_world()
    {
        var player = new InMemoryPlayerContext
        {
            Status = new PlayerStatus(1.5f, 2.25f, -3f, "Europa", null, 5, null),
        };

        var result = Call(Tools(player, new InMemoryWorldContext(), new InMemoryIcDevices(), new InMemoryMipsReference()), "describe_player");
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("Position: (1.5, 2.3, -3.0)");
        result.Content.ShouldContain("World: Europa");
        result.Content.ShouldContain("Room: 5");
    }

    [Test]
    public void Describe_player_unavailable_fails()
    {
        var result = Call(Tools(), "describe_player");
        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Get_world_reports_setup_orbit_and_air()
    {
        var world = new InMemoryWorldContext
        {
            Info = new WorldInfo(
                "Europa",
                "Europa1",
                -9.8f,
                12,
                false,
                false,
                "20m",
                "60d",
                72.0,
                0.01f,
                5.2,
                3.1f,
                false,
                500f,
                0.37f,
                0.35f,
                false,
                -20.0,
                0.0,
                new[] { new GasMixEntry("Nitrogen", 90.0), new GasMixEntry("Oxygen", 10.0) }),
        };

        var result = Call(Tools(new InMemoryPlayerContext(), world, new InMemoryIcDevices(), new InMemoryMipsReference()), "get_world");
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("World: Europa");
        result.Content.ShouldContain("Day length: 20m");
        result.Content.ShouldContain("500 W/m");
        result.Content.ShouldContain("Nitrogen 90%");
    }

    [Test]
    public void Get_world_unavailable_fails()
    {
        var result = Call(Tools(), "get_world");
        result.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public void Write_ic_places_code_in_the_named_editor()
    {
        var computers = new InMemoryIcDevices();

        var result = Call(
            Tools(new InMemoryPlayerContext(), new InMemoryWorldContext(), computers, new InMemoryMipsReference()),
            "write_ic",
            ("computer", "1189"),
            ("code", "alias Device d0\n yield"));

        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("Wrote 2 lines to Lab Computer");
        computers.Computer.Writes.Count.ShouldBe(1);
        computers.ComputerLookups[0].ShouldBe(1189L);
    }

    [Test]
    public void Write_ic_passes_thing_ids_through()
    {
        var computers = new InMemoryIcDevices();

        var result = Call(
            Tools(new InMemoryPlayerContext(), new InMemoryWorldContext(), computers, new InMemoryMipsReference()),
            "write_ic",
            ("computer", "1189"),
            ("code", "yield"));

        result.IsSuccess.ShouldBeTrue();
        computers.ComputerLookups[0].ShouldBe(1189L);
    }

    [Test]
    public void Write_ic_missing_computer_fails()
    {
        var result = Call(Tools(), "write_ic", ("code", "yield"));
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("'computer'");
    }

    [Test]
    public void Write_ic_unknown_computer_fails()
    {
        var computers = new InMemoryIcDevices { ComputerFound = false };

        var result = Call(
            Tools(new InMemoryPlayerContext(), new InMemoryWorldContext(), computers, new InMemoryMipsReference()),
            "write_ic",
            ("computer", "999"),
            ("code", "yield"));

        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("999");
    }

    [Test]
    public void Write_ic_refuses_a_computer_label()
    {
        var computers = new InMemoryIcDevices();

        var result = Call(
            Tools(new InMemoryPlayerContext(), new InMemoryWorldContext(), computers, new InMemoryMipsReference()),
            "write_ic",
            ("computer", "Lab Computer"),
            ("code", "yield"));

        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("is not a thing id");
        computers.ComputerLookups.Count.ShouldBe(0);
    }

    [Test]
    public void List_instructions_lists_names_on_blank()
    {
        var mips = new InMemoryMipsReference();
        mips.Instructions.Add(new MipsInstruction("l", "l r? device var", "Loads a value."));
        mips.Instructions.Add(new MipsInstruction("yield", "yield", "Pauses the script."));

        var result = Call(
            Tools(new InMemoryPlayerContext(), new InMemoryWorldContext(), new InMemoryIcDevices(), mips),
            "list_instructions");

        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldBe("l\nyield");
    }

    [Test]
    public void List_instructions_describes_one_instruction()
    {
        var mips = new InMemoryMipsReference();
        mips.Instructions.Add(new MipsInstruction("l", "l r? device var", "Loads a value."));

        var result = Call(
            Tools(new InMemoryPlayerContext(), new InMemoryWorldContext(), new InMemoryIcDevices(), mips),
            "list_instructions",
            ("instruction", "L"));

        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("Loads a value.");
    }

    [Test]
    public void List_instructions_unknown_instruction_fails()
    {
        var result = Call(Tools(), "list_instructions", ("instruction", "nope"));
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("nope");
    }

    [Test]
    public void Search_devices_lists_name_type_and_network()
    {
        var devices = new InMemoryDeviceCatalog();
        devices.Devices.Add(new DeviceInfo("Air Vent", "StructureAirVent", 101, 7, true, 5));
        devices.Devices.Add(new DeviceInfo("Gas Sensor", "StructureGasSensor", 102, 0, true, 5));

        var result = Call(
            Tools(new InMemoryPlayerContext(), new InMemoryWorldContext(), new InMemoryIcDevices(), new InMemoryMipsReference(), devices),
            "search_devices",
            ("query", "vent"));

        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("| Air Vent | StructureAirVent |   101 |       7 | yes   |    5 |");
        result.Content.ShouldNotContain("Gas Sensor");
    }

    [Test]
    public void Search_devices_passes_the_network_through()
    {
        var devices = new InMemoryDeviceCatalog();

        var result = Call(
            Tools(new InMemoryPlayerContext(), new InMemoryWorldContext(), new InMemoryIcDevices(), new InMemoryMipsReference(), devices),
            "search_devices",
            ("network", "7"));

        result.IsSuccess.ShouldBeTrue();
        devices.LastNetwork.ShouldBe(7);
    }

    [Test]
    public void Search_devices_filters_by_network()
    {
        var devices = new InMemoryDeviceCatalog();
        devices.Devices.Add(new DeviceInfo("Air Vent", "StructureAirVent", 101, 7, true, 5));
        devices.Devices.Add(new DeviceInfo("Gas Sensor", "StructureGasSensor", 102, 0, true, 5));

        var result = Call(
            Tools(new InMemoryPlayerContext(), new InMemoryWorldContext(), new InMemoryIcDevices(), new InMemoryMipsReference(), devices),
            "search_devices",
            ("network", "7"));

        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("Air Vent");
        result.Content.ShouldNotContain("Gas Sensor");
    }

    [Test]
    public void Search_devices_filters_by_logic()
    {
        var devices = new InMemoryDeviceCatalog();
        devices.Devices.Add(new DeviceInfo("Air Vent", "StructureAirVent", 101, 7, true, 5));
        devices.Devices.Add(new DeviceInfo("Iron Pipe", "StructurePipe", 103, 0, false, 0));

        var smart = Call(
            Tools(new InMemoryPlayerContext(), new InMemoryWorldContext(), new InMemoryIcDevices(), new InMemoryMipsReference(), devices),
            "search_devices",
            ("logic", "true"));
        smart.IsSuccess.ShouldBeTrue();
        smart.Content.ShouldContain("Air Vent");
        smart.Content.ShouldNotContain("Iron Pipe");
        devices.LastLogic.ShouldBe(true);

        var dumb = Call(
            Tools(new InMemoryPlayerContext(), new InMemoryWorldContext(), new InMemoryIcDevices(), new InMemoryMipsReference(), devices),
            "search_devices",
            ("logic", "no"));
        dumb.IsSuccess.ShouldBeTrue();
        dumb.Content.ShouldContain("Iron Pipe");
        dumb.Content.ShouldNotContain("Air Vent");
    }

    [Test]
    public void Search_devices_rejects_bad_logic()
    {
        var result = Call(Tools(), "search_devices", ("logic", "maybe"));
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("'maybe'");
    }

    [Test]
    public void Search_devices_filters_by_room()
    {
        var devices = new InMemoryDeviceCatalog();
        devices.Devices.Add(new DeviceInfo("Air Vent", "StructureAirVent", 101, 7, true, 5));
        devices.Devices.Add(new DeviceInfo("Iron Pipe", "StructurePipe", 103, 0, false, 0));

        var result = Call(
            Tools(new InMemoryPlayerContext(), new InMemoryWorldContext(), new InMemoryIcDevices(), new InMemoryMipsReference(), devices),
            "search_devices",
            ("room", "5"));

        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("Air Vent");
        result.Content.ShouldNotContain("Iron Pipe");
        devices.LastRoom.ShouldBe(5);
    }

    [Test]
    public void Search_devices_rejects_a_bad_room()
    {
        var result = Call(Tools(), "search_devices", ("room", "abc"));
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("'abc'");
    }

    [Test]
    public void Search_devices_rejects_a_bad_network()
    {
        var result = Call(Tools(), "search_devices", ("network", "abc"));
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("'abc'");
    }

    [Test]
    public void Search_devices_empty_reports_no_devices()
    {
        var result = Call(Tools(), "search_devices", ("query", "nope"));
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("No devices found for 'nope'.");
    }

    [Test]
    public void Describe_device_reports_logic_and_slots()
    {
        var inspector = new InMemoryDeviceInspector
        {
            Details = new DeviceDetails(
                "Air Vent",
                "StructureAirVent",
                101,
                7,
                true,
                new[] { new DeviceLogicValue("Pressure", 101.3, false), new DeviceLogicValue("On", 1, true) },
                new[] { new DeviceSlotInfo(0, "Filter", "empty") },
                null,
                null,
                null),
        };
        var tools = new ToolHandler(new ITool[]
        {
            new DescribeDeviceTool(new DescribeDeviceCommand(inspector, new InMemoryDeviceCatalog())),
        });

        var result = Call(tools, "describe_device", ("device", "Air Vent"));
        result.IsSuccess.ShouldBeTrue();
        inspector.LastDevice.ShouldBe("Air Vent");
        result.Content.ShouldContain("Device: Air Vent (StructureAirVent #101)");
        result.Content.ShouldContain("Network: 7, On: yes");
        result.Content.ShouldContain("Pressure");
        result.Content.ShouldContain("Filter");
    }

    [Test]
    public void Describe_device_lists_housing_devices()
    {
        var inspector = new InMemoryDeviceInspector
        {
            Details = new DeviceDetails(
                "Housing",
                "StructureCircuitHousing",
                100,
                7,
                null,
                new DeviceLogicValue[0],
                new DeviceSlotInfo[0],
                new[]
                {
                    new HousingDeviceEntry("db", "Housing", 100),
                    new HousingDeviceEntry("d0", "Heater", 101),
                    new HousingDeviceEntry("d1", "empty", 0),
                },
                new ChipState("IncorrectVariable", "12", 294, "6c96f2ae"),
                null),
        };
        var tools = new ToolHandler(new ITool[]
        {
            new DescribeDeviceTool(new DescribeDeviceCommand(inspector, new InMemoryDeviceCatalog())),
        });

        var result = Call(tools, "describe_device", ("device", "Housing"));
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("Devices:");
        result.Content.ShouldContain("Heater #101");
        result.Content.ShouldContain("Error: IncorrectVariable at line 12");
        result.Content.ShouldContain("Display: 294");
        result.Content.ShouldContain("Code hash: 6c96f2ae");
    }

    [Test]
    public void Describe_device_missing_argument_fails()
    {
        var tools = new ToolHandler(new ITool[]
        {
            new DescribeDeviceTool(new DescribeDeviceCommand(new InMemoryDeviceInspector(), new InMemoryDeviceCatalog())),
        });

        var result = Call(tools, "describe_device");
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("'device'");
    }

    [Test]
    public void Describe_device_unknown_device_fails()
    {
        var tools = new ToolHandler(new ITool[]
        {
            new DescribeDeviceTool(new DescribeDeviceCommand(new InMemoryDeviceInspector(), new InMemoryDeviceCatalog())),
        });

        var result = Call(tools, "describe_device", ("device", "Nope"));
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("Nope");
    }

    [Test]
    public void Write_ic_rejects_code_over_the_line_cap()
    {
        var computers = new InMemoryIcDevices();
        var tools = new ToolHandler(new ITool[]
        {
            new WriteIcTool(new UseComputerCommand(computers)),
        });
        var code = string.Join("\n", System.Linq.Enumerable.Repeat("yield", WriteIcTool.MaxCodeLines + 1));

        var result = Call(tools, "write_ic", ("computer", "1189"), ("code", code));
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("128 lines");
        computers.Computer.Writes.Count.ShouldBe(0);
    }

    [Test]
    public void Export_ic_burns_and_verifies()
    {
        var computers = new InMemoryIcDevices();
        var tools = new ToolHandler(new ITool[]
        {
            new ExportIcTool(new UseComputerCommand(computers)),
        });

        var result = Call(tools, "export_ic", ("computer", "1189"), ("device", "2200"));
        result.IsSuccess.ShouldBeTrue();
        result.Content.ShouldContain("Exported 37 lines to Climate Control. Chip hash matches the editor.");
        computers.ComputerLookups[0].ShouldBe(1189L);
        computers.Computer.Exports.Count.ShouldBe(1);
        computers.Computer.Exports[0].ShouldBe(2200L);
    }

    [Test]
    public void Export_ic_missing_device_fails()
    {
        var tools = new ToolHandler(new ITool[]
        {
            new ExportIcTool(new UseComputerCommand(new InMemoryIcDevices())),
        });

        var result = Call(tools, "export_ic", ("computer", "1189"));
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("'device'");
    }

    [Test]
    public void Export_ic_failed_burn_fails()
    {
        var tools = new ToolHandler(new ITool[]
        {
            new ExportIcTool(new UseComputerCommand(new InMemoryIcDevices
            {
                Computer = { Result = new IcWriteResult(false, null, 0, "Chip reports IncorrectVariable at line 12.") },
            })),
        });

        var result = Call(tools, "export_ic", ("computer", "1189"), ("device", "2200"));
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("IncorrectVariable");
    }

    [Test]
    public void Unknown_tool_fails()
    {
        var result = Tools().Handle(new ToolCall("1", "search_bosses", ToolArguments.Empty));
        result.IsSuccess.ShouldBeFalse();
        result.Content.ShouldContain("search_bosses");
    }
}
