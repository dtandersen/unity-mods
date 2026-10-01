# Station Automation & Guidance Engine (SAGE)

Sage is an assistant for Stationeers, shipped as a BepInEx 5 plugin. Ask it
questions and it looks up live game data — items, structures, manufacturing
recipes, Stationpedia pages, your inventory, and your position — before
answering.

## Usage

There are two ways to interact with SAGE. You can mention `@sage <prompt>` in a chat message or use `ai <prompt>` in the Stationeers console.

```
@sage how do I make cable coil?
ai what does the autolathe need for steel sheets?
```

SAGE posts the answer to chat — on a server, every player sees it. Two settings control mention handling:
`General/RespondToMentions` (default on) and `General/MentionTrigger`
(default `@sage`).

For longer sessions with history, use the console (default key F3, rebindable
in settings):

```
ai how do I make cable coil?
ai clear     # drop conversation history
ai compact   # summarise history into one message
```

First set `AI/ApiKey` (and optionally `Endpoint`/`Model`) in the BepInEx
config (`BepInEx/config/davidandersen.stationeersai.cfg`) — any
OpenAI-compatible endpoint works.

Sage can call these tools:

| Tool | Data |
|---|---|
| `search_items` | Items by name (from the prefab catalogue) |
| `search_structures` | Buildable structures by name |
| `search_recipes` | Manufacturing recipes with station + ingredients |
| `search_stationpedia` / `read_stationpedia` | In-game encyclopedia pages |
| `search_inventory` | What you are carrying and where it is worn |
| `get_status` | Your position and world |
| `get_world` | World, orbit, sunlight, and outside air |

## Development

Building, testing, and architecture notes live in [development.md](development.md).

## License

[MIT](license.md). Use at your own risk.
