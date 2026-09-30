using Shouldly;
using StationeersAi.Command;
using StationeersAi.Entity;
using StationeersAi.Query;
using StationeersAi.Repository;
using StationeersAi.Tool;

namespace StationeersAi.Test
{
    /// <summary>Tool argument handling, failure paths, and model-facing text.</summary>
    public class ToolTests
    {
        private static ToolHandler Tools(InMemoryPlayerContext player = null)
        {
            player ??= new InMemoryPlayerContext();

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
                new GetStatusTool(new GetStatusCommand(player)),
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

            var result = Call(Tools(player), "search_inventory", ("query", "weld"));
            result.IsSuccess.ShouldBeTrue();
            result.Content.ShouldContain("Welder,85%,Right hand");
        }

        [Test]
        public void Get_status_reports_position_and_world()
        {
            var player = new InMemoryPlayerContext
            {
                Status = new PlayerStatus(1.5f, 2.25f, -3f, "Europa", null),
            };

            var result = Call(Tools(player), "get_status");
            result.IsSuccess.ShouldBeTrue();
            result.Content.ShouldContain("Position: (1.5, 2.3, -3.0)");
            result.Content.ShouldContain("World: Europa");
        }

        [Test]
        public void Get_status_unavailable_fails()
        {
            var result = Call(Tools(), "get_status");
            result.IsSuccess.ShouldBeFalse();
        }

        [Test]
        public void Unknown_tool_fails()
        {
            var result = Tools().Handle(new ToolCall("1", "search_bosses", ToolArguments.Empty));
            result.IsSuccess.ShouldBeFalse();
            result.Content.ShouldContain("search_bosses");
        }
    }
}
