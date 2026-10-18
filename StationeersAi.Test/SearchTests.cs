using System.Collections.Generic;
using Shouldly;
using StationeersAi.Entity;
using StationeersAi.Query;
using StationeersAi.Repository;

namespace StationeersAi.Test;

/// <summary>Search semantics shared by every index: case-insensitive substring.</summary>
public class SearchTests
{
    private static InMemoryItemRepository Items() => new InMemoryItemRepository(new[]
    {
        new Item("ItemSteel", "Steel", "Ingot"),
        new Item("ItemSteelSheets", "Steel Sheets", "Ingot"),
        new Item("ItemCopper", "Copper", "Ore"),
    });

    private static InMemoryStructureRepository Structures() => new InMemoryStructureRepository(new[]
    {
        new Structure("StructureSolarPanel", "Solar Panel", "Generates power.", new BuildStep[0]),
        new Structure("StructureSolidFuelGenerator", "Solid Fuel Generator", "Burns coal.", new BuildStep[0]),
    });

    private static InMemoryRecipeRepository Recipes() => new InMemoryRecipeRepository(new[]
    {
        new Recipe("ItemCableCoil", "ItemCableCoil", "Cable Coil", 1, "Electronics Printer",
            new[] { new Ingredient("Copper", 5) }),
        new Recipe("ItemSteelSheets", "ItemSteelSheets", "Steel Sheets", 1, "Hydraulic Pipe Bender",
            new[] { new Ingredient("Steel", 1) }),
    });

    private static InMemoryStationpediaRepository Pages() => new InMemoryStationpediaRepository(new[]
    {
        new StationpediaEntry("ItemSteel", "Steel", "A sturdy alloy.", "Smelt iron and coal..."),
        new StationpediaEntry("GasOxygen", "Oxygen", "Breathe it.", "Oxygen supports..."),
    });

    [Test]
    public void Item_search_matches_substring_case_insensitively()
    {
        var index = new ItemIndex(Items());
        index.Search("steel").Count.ShouldBe(2);
        index.Search("STEEL").Count.ShouldBe(2);
        index.Search("copper")[0].PrefabName.ShouldBe("ItemCopper");
    }

    [Test]
    public void Item_search_empty_query_finds_nothing()
    {
        var index = new ItemIndex(Items());
        index.Search("").ShouldBeEmpty();
        index.Search("  ").ShouldBeEmpty();
        index.Search(null).ShouldBeEmpty();
        index.Search("plastic").ShouldBeEmpty();
    }

    [Test]
    public void Structure_search_matches_by_name()
    {
        var index = new StructureIndex(Structures());
        index.Search("solar").Count.ShouldBe(1);
        index.Search("generator")[0].PrefabName.ShouldBe("StructureSolidFuelGenerator");
        index.Search("airlock").ShouldBeEmpty();
    }

    [Test]
    public void Recipe_search_matches_by_result_name()
    {
        var index = new RecipeIndex(Recipes());
        index.Search("cable").Count.ShouldBe(1);
        index.Search("steel")[0].Station.ShouldBe("Hydraulic Pipe Bender");
        index.Search("glass").ShouldBeEmpty();
    }

    [Test]
    public void Stationpedia_search_matches_title_or_key()
    {
        var index = new StationpediaIndex(Pages());
        index.Search("steel").Count.ShouldBe(1);
        index.Search("GasOxygen").Count.ShouldBe(1);
        index.Search("nitrogen").ShouldBeEmpty();
    }

    [Test]
    public void Inventory_search_matches_carried_names()
    {
        var player = new InMemoryPlayerContext();
        player.Carry(new ItemStack("ItemWelder", "Welder", "85%", "Right hand"));
        player.Carry(new ItemStack("ItemSteel", "Steel", "x10", "Backpack"));

        var index = new InventoryIndex(player);
        index.Search("weld").Count.ShouldBe(1);
        index.Search("steel")[0].Slot.ShouldBe("Backpack");
        index.Search("helmet").ShouldBeEmpty();
    }
}
